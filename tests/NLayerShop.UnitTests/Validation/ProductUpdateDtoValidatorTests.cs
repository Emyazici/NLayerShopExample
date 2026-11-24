using FluentValidation.TestHelper;
using NLayerShop.Business.Validation;
using NLayerShop.Contracts.DTOs.Products;

namespace NLayerShop.UnitTests.Validation;

public class ProductUpdateDtoValidatorTests
{
    private readonly ProductUpdateDtoValidator _validator = new();

    // SQL Server rowversion is exactly 8 bytes.
    private static readonly string ValidRowVersion = Convert.ToBase64String(new byte[] { 0, 0, 0, 0, 0, 0, 7, 209 });

    private static ProductUpdateDto Valid() => new("Phone", 10m, "Smartphone", 5, null, ValidRowVersion);

    [Fact]
    public void Valid_dto_passes()
    {
        _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Missing_row_version_fails(string? rowVersion)
    {
        _validator.TestValidate(Valid() with { RowVersion = rowVersion! }).ShouldHaveValidationErrorFor(x => x.RowVersion);
    }

    [Fact]
    public void Non_base64_row_version_fails()
    {
        _validator.TestValidate(Valid() with { RowVersion = "not-base64!!" })
            .ShouldHaveValidationErrorFor(x => x.RowVersion)
            .WithErrorMessage("'Row Version' must be a valid Base64-encoded rowversion value.");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(9)]
    public void Row_version_with_wrong_length_fails(int byteCount)
    {
        var rowVersion = Convert.ToBase64String(new byte[byteCount]);

        _validator.TestValidate(Valid() with { RowVersion = rowVersion }).ShouldHaveValidationErrorFor(x => x.RowVersion);
    }

    [Fact]
    public void Empty_name_fails()
    {
        _validator.TestValidate(Valid() with { Name = "" }).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Negative_price_and_stock_fail()
    {
        var result = _validator.TestValidate(Valid() with { Price = -1m, StockCount = -1 });

        result.ShouldHaveValidationErrorFor(x => x.Price);
        result.ShouldHaveValidationErrorFor(x => x.StockCount);
    }

    [Fact]
    public void Zero_category_id_fails()
    {
        _validator.TestValidate(Valid() with { CategoryId = 0 }).ShouldHaveValidationErrorFor(x => x.CategoryId);
    }
}
