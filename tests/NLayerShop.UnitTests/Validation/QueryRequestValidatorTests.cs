using FluentValidation.TestHelper;
using NLayerShop.Business.Validation;
using NLayerShop.Contracts.Common;
using NLayerShop.Contracts.DTOs.Orders;

namespace NLayerShop.UnitTests.Validation;

public class PagedRequestValidatorTests
{
    private readonly PagedRequestValidator _validator = new();

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 20)]
    [InlineData(50, 100)]
    public void Valid_paging_passes(int page, int size)
    {
        _validator.TestValidate(new PagedRequest { Page = page, Size = size }).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Page_below_one_fails(int page)
    {
        _validator.TestValidate(new PagedRequest { Page = page }).ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Size_outside_1_to_100_fails(int size)
    {
        _validator.TestValidate(new PagedRequest { Size = size }).ShouldHaveValidationErrorFor(x => x.Size);
    }
}

public class ProductQueryRequestValidatorTests
{
    private readonly ProductQueryRequestValidator _validator = new();

    [Fact]
    public void Default_request_passes()
    {
        _validator.TestValidate(new ProductQueryRequest()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Paging_rules_are_included()
    {
        var result = _validator.TestValidate(new ProductQueryRequest { Page = 0, Size = 101 });

        result.ShouldHaveValidationErrorFor(x => x.Page);
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("price")]
    [InlineData("createdAt")]
    [InlineData("PRICE")]
    [InlineData("createdat")]
    public void Whitelisted_sort_fields_pass_case_insensitively(string sortBy)
    {
        _validator.TestValidate(new ProductQueryRequest { SortBy = sortBy }).ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Theory]
    [InlineData("stock")]
    [InlineData("id")]
    [InlineData("name;drop table")]
    public void Unknown_sort_field_fails(string sortBy)
    {
        _validator.TestValidate(new ProductQueryRequest { SortBy = sortBy }).ShouldHaveValidationErrorFor(x => x.SortBy);
    }

    [Theory]
    [InlineData("asc")]
    [InlineData("DESC")]
    public void Valid_sort_direction_passes(string sortDir)
    {
        _validator.TestValidate(new ProductQueryRequest { SortDir = sortDir }).ShouldNotHaveValidationErrorFor(x => x.SortDir);
    }

    [Fact]
    public void Unknown_sort_direction_fails()
    {
        _validator.TestValidate(new ProductQueryRequest { SortDir = "up" }).ShouldHaveValidationErrorFor(x => x.SortDir);
    }

    [Fact]
    public void Min_price_greater_than_max_price_fails()
    {
        _validator.TestValidate(new ProductQueryRequest { MinPrice = 50, MaxPrice = 10 })
            .ShouldHaveValidationErrorFor(x => x.MaxPrice);
    }

    [Fact]
    public void Min_price_equal_to_max_price_passes()
    {
        _validator.TestValidate(new ProductQueryRequest { MinPrice = 10, MaxPrice = 10 }).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Negative_prices_fail()
    {
        var result = _validator.TestValidate(new ProductQueryRequest { MinPrice = -1, MaxPrice = -1 });

        result.ShouldHaveValidationErrorFor(x => x.MinPrice);
        result.ShouldHaveValidationErrorFor(x => x.MaxPrice);
    }

    [Fact]
    public void Search_longer_than_limit_fails()
    {
        var search = new string('s', ProductQueryRequestValidator.MaxSearchLength + 1);

        _validator.TestValidate(new ProductQueryRequest { Search = search }).ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Fact]
    public void Zero_category_id_fails()
    {
        _validator.TestValidate(new ProductQueryRequest { CategoryId = 0 }).ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}

public class OrderQueryRequestValidatorTests
{
    private readonly OrderQueryRequestValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("Pending")]
    [InlineData("completed")]
    [InlineData("CANCELLED")]
    public void Known_or_missing_status_passes(string? status)
    {
        _validator.TestValidate(new OrderQueryRequest { Status = status }).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Shipped")]
    [InlineData("1")]
    [InlineData("5")]
    public void Unknown_or_numeric_status_fails(string status)
    {
        _validator.TestValidate(new OrderQueryRequest { Status = status }).ShouldHaveValidationErrorFor(x => x.Status);
    }
}
