using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NLayerShop.Contracts.DTOs.Categories;
using NLayerShop.Contracts.DTOs.Orders;
using NLayerShop.Contracts.DTOs.Products;

namespace NLayerShop.IntegrationTests.Infrastructure;

/// <summary>Paged response shape as it appears on the wire (including computed metadata).</summary>
public sealed record PageDto<T>(
    List<T> Items,
    int Page,
    int Size,
    int TotalCount,
    int TotalPages,
    bool HasPrevious,
    bool HasNext);

[Collection(ApiCollection.Name)]
public abstract class ApiTestBase
{
    protected ApiTestBase(ApiFactory factory)
    {
        Client = factory.CreateClient();
    }

    protected HttpClient Client { get; }

    /// <summary>Unique, readable value so tests never see each other's data.</summary>
    protected static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    protected async Task<CategoryDetailDto> CreateCategoryAsync(string? name = null)
    {
        var response = await Client.PostAsJsonAsync("/api/categories", new CategoryCreateDto(name ?? Unique("cat")));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CategoryDetailDto>())!;
    }

    protected async Task<ProductDetailDto> CreateProductAsync(
        string? name = null, decimal price = 10m, int stock = 10, int? categoryId = null)
    {
        var dto = new ProductCreateDto(name ?? Unique("product"), price, "integration test product", stock, categoryId);
        var response = await Client.PostAsJsonAsync("/api/products", dto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ProductDetailDto>())!;
    }

    protected async Task<ProductDetailDto> GetProductAsync(int id)
    {
        var response = await Client.GetAsync($"/api/products/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ProductDetailDto>())!;
    }

    protected Task<HttpResponseMessage> PostOrderAsync(params (int ProductId, int Quantity)[] lines) =>
        Client.PostAsJsonAsync("/api/orders",
            new OrderCreateDto(lines.Select(l => new OrderItemCreateDto(l.ProductId, l.Quantity)).ToList()));

    protected async Task<OrderDetailDto> CreateOrderAsync(params (int ProductId, int Quantity)[] lines)
    {
        var response = await PostOrderAsync(lines);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<OrderDetailDto>())!;
    }

    protected async Task<OrderDetailDto> GetOrderAsync(int id)
    {
        var response = await Client.GetAsync($"/api/orders/{id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<OrderDetailDto>())!;
    }

    protected static async Task<ProblemDetails> ShouldBeProblemAsync(HttpResponseMessage response, HttpStatusCode status)
    {
        response.StatusCode.Should().Be(status);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)status);
        return problem;
    }

    protected static async Task<ValidationProblemDetails> ShouldBeValidationProblemAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        return problem!;
    }
}
