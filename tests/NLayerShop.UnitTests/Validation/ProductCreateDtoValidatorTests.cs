using FluentValidation.TestHelper;
using NLayerShop.Business.Validation;
using NLayerShop.Contracts.DTOs.Products;

namespace NLayerShop.UnitTests.Validation;

public class ProductCreateDtoValidatorTests
{
    private readonly ProductCreateDtoValidator _validator = new();

    private static ProductCreateDto Valid() => new("Phone", 999.90m, "Smartphone", 10, 1);

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Null_category_is_allowed()
    {
        _validator.TestValidate(Valid() with { CategoryId = null }).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_name_fails(string name)
    {
        _validator.TestValidate(Valid() with { Name = name }).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Name_longer_than_100_characters_fails()
    {
        _validator.TestValidate(Valid() with { Name = new string('a', 101) }).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Null_description_fails()
    {
        _validator.TestValidate(Valid() with { Description = null! }).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Description_longer_than_1000_characters_fails()
    {
        _validator.TestValidate(Valid() with { Description = new string('d', 1001) })
            .ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Negative_price_fails()
    {
        _validator.TestValidate(Valid() with { Price = -0.01m }).ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Negative_stock_fails()
    {
        _validator.TestValidate(Valid() with { StockCount = -1 }).ShouldHaveValidationErrorFor(x => x.StockCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Non_positive_category_id_fails(int categoryId)
    {
        _validator.TestValidate(Valid() with { CategoryId = categoryId }).ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}
