using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using NLayerShop.Contracts.DTOs.Products;
using NLayerShop.IntegrationTests.Infrastructure;

namespace NLayerShop.IntegrationTests;

public class ProductsApiTests : ApiTestBase
{
    public ProductsApiTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Create_returns_201_with_location_and_get_returns_the_product()
    {
        var category = await CreateCategoryAsync();
        var name = Unique("phone");

        var response = await Client.PostAsJsonAsync("/api/products",
            new ProductCreateDto(name, 999.90m, "Smartphone", 7, category.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var created = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        created!.Id.Should().BePositive();
        created.RowVersion.Should().NotBeNullOrEmpty();
        Convert.FromBase64String(created.RowVersion).Should().HaveCount(8);

        var fetched = await Client.GetFromJsonAsync<ProductDetailDto>(response.Headers.Location);
        fetched.Should().BeEquivalentTo(created);
        fetched!.Name.Should().Be(name);
        fetched.Price.Should().Be(999.90m);
        fetched.StockCount.Should().Be(7);
        fetched.CategoryName.Should().Be(category.Name);
    }

    [Fact]
    public async Task Update_replaces_all_fields_and_changes_row_version()
    {
        var product = await CreateProductAsync(price: 5m, stock: 1, categoryId: (await CreateCategoryAsync()).Id);
        var newName = Unique("renamed");

        var response = await Client.PutAsJsonAsync($"/api/products/{product.Id}",
            new ProductUpdateDto(newName, 12.5m, "updated", 3, null, product.RowVersion));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updated = await GetProductAsync(product.Id);
        updated.Name.Should().Be(newName);
        updated.Price.Should().Be(12.5m);
        updated.Description.Should().Be("updated");
        updated.StockCount.Should().Be(3);
        updated.CategoryId.Should().BeNull("PUT is a full replace and categoryId was null");
        updated.RowVersion.Should().NotBe(product.RowVersion);
    }

    [Fact]
    public async Task Delete_soft_deletes_so_product_is_neither_fetchable_nor_listed()
    {
        var tag = Unique("softdel");
        var kept = await CreateProductAsync(name: $"{tag}-kept");
        var deleted = await CreateProductAsync(name: $"{tag}-deleted");

        var response = await Client.DeleteAsync($"/api/products/{deleted.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await ShouldBeProblemAsync(await Client.GetAsync($"/api/products/{deleted.Id}"), HttpStatusCode.NotFound);

        var page = await Client.GetFromJsonAsync<PageDto<ProductDetailDto>>($"/api/products?search={tag}");
        page!.TotalCount.Should().Be(1);
        page.Items.Select(p => p.Id).Should().Equal(kept.Id);

        await ShouldBeProblemAsync(await Client.DeleteAsync($"/api/products/{deleted.Id}"), HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_sorts_and_returns_paging_metadata()
    {
        var tag = Unique("paging");
        var p30 = await CreateProductAsync(name: $"{tag}-c", price: 30m);
        var p10 = await CreateProductAsync(name: $"{tag}-a", price: 10m);
        var p20 = await CreateProductAsync(name: $"{tag}-b", price: 20m);

        var first = await Client.GetFromJsonAsync<PageDto<ProductDetailDto>>(
            $"/api/products?search={tag}&sortBy=price&sortDir=asc&page=1&size=2");

        first!.Items.Select(p => p.Id).Should().Equal(p10.Id, p20.Id);
        first.Page.Should().Be(1);
        first.Size.Should().Be(2);
        first.TotalCount.Should().Be(3);
        first.TotalPages.Should().Be(2);
        first.HasPrevious.Should().BeFalse();
        first.HasNext.Should().BeTrue();

        var second = await Client.GetFromJsonAsync<PageDto<ProductDetailDto>>(
            $"/api/products?search={tag}&sortBy=price&sortDir=asc&page=2&size=2");

        second!.Items.Select(p => p.Id).Should().Equal(p30.Id);
        second.HasPrevious.Should().BeTrue();
        second.HasNext.Should().BeFalse();

        var byNameDesc = await Client.GetFromJsonAsync<PageDto<ProductDetailDto>>(
            $"/api/products?search={tag}&sortBy=name&sortDir=desc");

        byNameDesc!.Items.Select(p => p.Id).Should().Equal(p30.Id, p20.Id, p10.Id);

        var newestFirst = await Client.GetFromJsonAsync<PageDto<ProductDetailDto>>($"/api/products?search={tag}");

        newestFirst!.Items.Select(p => p.Id).Should().Equal(new[] { p20.Id, p10.Id, p30.Id },
            "the default sort is createdAt desc with id as tie-breaker");
    }

    [Fact]
    public async Task List_filters_by_price_range_stock_and_category()
    {
        var tag = Unique("filter");
        var category = await CreateCategoryAsync();
        var cheap = await CreateProductAsync(name: $"{tag}-cheap", price: 5m, stock: 3);
        var mid = await CreateProductAsync(name: $"{tag}-mid", price: 50m, stock: 3, categoryId: category.Id);
        var midOut = await CreateProductAsync(name: $"{tag}-mid-out", price: 60m, stock: 0, categoryId: category.Id);
        var pricey = await CreateProductAsync(name: $"{tag}-pricey", price: 500m, stock: 3);

        var inRange = await Client.GetFromJsonAsync<PageDto<ProductDetailDto>>(
            $"/api/products?search={tag}&minPrice=10&maxPrice=100&sortBy=price");
        inRange!.Items.Select(p => p.Id).Should().Equal(mid.Id, midOut.Id);

        var inStock = await Client.GetFromJsonAsync<PageDto<ProductDetailDto>>(
            $"/api/products?search={tag}&inStock=true&sortBy=price");
        inStock!.Items.Select(p => p.Id).Should().Equal(cheap.Id, mid.Id, pricey.Id);

        var outOfStock = await Client.GetFromJsonAsync<PageDto<ProductDetailDto>>(
            $"/api/products?search={tag}&inStock=false");
        outOfStock!.Items.Select(p => p.Id).Should().Equal(midOut.Id);

        var inCategory = await Client.GetFromJsonAsync<PageDto<ProductDetailDto>>(
            $"/api/products?categoryId={category.Id}&sortBy=price");
        inCategory!.Items.Select(p => p.Id).Should().Equal(mid.Id, midOut.Id);
        inCategory.Items.Should().OnlyContain(p => p.CategoryName == category.Name);
    }

    [Fact]
    public async Task Create_with_invalid_body_returns_400_with_field_errors()
    {
        var response = await Client.PostAsJsonAsync("/api/products",
            new ProductCreateDto("", -1m, "x", -5, null));

        var problem = await ShouldBeValidationProblemAsync(response);
        problem.Errors.Keys.Select(k => k.ToLowerInvariant())
            .Should().Contain(new[] { "name", "price", "stockcount" });
    }

    [Fact]
    public async Task Create_without_price_and_stock_returns_400_instead_of_defaulting_to_zero()
    {
        var response = await Client.PostAsJsonAsync("/api/products",
            new { name = Unique("no-price"), description = "x", categoryId = (int?)null });

        var problem = await ShouldBeValidationProblemAsync(response);
        // JsonRequired failures are reported by the JSON input formatter (key "$"), with the
        // missing property names in the message, so search keys and messages together.
        var text = string.Join(" ", problem.Errors.Keys.Concat(problem.Errors.Values.SelectMany(v => v)))
            .ToLowerInvariant();
        text.Should().Contain("price").And.Contain("stockcount");
    }

    [Fact]
    public async Task List_with_invalid_sort_and_price_range_returns_400()
    {
        var response = await Client.GetAsync("/api/products?sortBy=stock&minPrice=50&maxPrice=10&size=101");

        var problem = await ShouldBeValidationProblemAsync(response);
        problem.Errors.Keys.Select(k => k.ToLowerInvariant())
            .Should().Contain(new[] { "sortby", "maxprice", "size" });
    }

    [Fact]
    public async Task Update_without_row_version_returns_400()
    {
        var product = await CreateProductAsync();

        var response = await Client.PutAsJsonAsync($"/api/products/{product.Id}",
            new ProductUpdateDto(product.Name, 1m, "x", 1, null, ""));

        var problem = await ShouldBeValidationProblemAsync(response);
        problem.Errors.Keys.Select(k => k.ToLowerInvariant()).Should().Contain("rowversion");
    }

    [Fact]
    public async Task Get_and_update_unknown_product_return_404()
    {
        await ShouldBeProblemAsync(await Client.GetAsync("/api/products/987654321"), HttpStatusCode.NotFound);

        var rowVersion = Convert.ToBase64String(new byte[8]);
        var update = await Client.PutAsJsonAsync("/api/products/987654321",
            new ProductUpdateDto("ghost", 1m, "x", 1, null, rowVersion));
        await ShouldBeProblemAsync(update, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Second_update_with_the_same_row_version_returns_409()
    {
        var product = await CreateProductAsync();

        var first = await Client.PutAsJsonAsync($"/api/products/{product.Id}",
            new ProductUpdateDto(Unique("first"), 1m, "first", 1, null, product.RowVersion));
        first.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var second = await Client.PutAsJsonAsync($"/api/products/{product.Id}",
            new ProductUpdateDto(Unique("second"), 2m, "second", 2, null, product.RowVersion));

        var problem = await ShouldBeProblemAsync(second, HttpStatusCode.Conflict);
        problem.Title.Should().Be("Concurrency conflict");
        (await GetProductAsync(product.Id)).Description.Should().Be("first");
    }

    [Fact]
    public async Task Create_and_update_with_unknown_category_return_422()
    {
        var create = await Client.PostAsJsonAsync("/api/products",
            new ProductCreateDto(Unique("orphan"), 1m, "x", 1, 987654321));
        var problem = await ShouldBeProblemAsync(create, HttpStatusCode.UnprocessableEntity);
        problem.Detail.Should().Contain("987654321");

        var product = await CreateProductAsync();
        var update = await Client.PutAsJsonAsync($"/api/products/{product.Id}",
            new ProductUpdateDto(product.Name, 1m, "x", 1, 987654321, product.RowVersion));
        await ShouldBeProblemAsync(update, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Create_with_soft_deleted_category_returns_422()
    {
        var category = await CreateCategoryAsync();
        (await Client.DeleteAsync($"/api/categories/{category.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await Client.PostAsJsonAsync("/api/products",
            new ProductCreateDto(Unique("late"), 1m, "x", 1, category.Id));

        await ShouldBeProblemAsync(response, HttpStatusCode.UnprocessableEntity);
    }
}
