using FluentValidation.TestHelper;
using NLayerShop.Business.Validation;
using NLayerShop.Contracts.DTOs.Orders;

namespace NLayerShop.UnitTests.Validation;

public class OrderCreateDtoValidatorTests
{
    private readonly OrderCreateDtoValidator _validator = new();

    private static OrderCreateDto Order(params OrderItemCreateDto[] items) => new(items.ToList());

    [Fact]
    public void Valid_order_passes()
    {
        _validator.TestValidate(Order(new(1, 2), new(2, 1))).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_items_fail()
    {
        _validator.TestValidate(Order())
            .ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("An order must contain at least one item.");
    }

    [Fact]
    public void Null_items_fail()
    {
        _validator.TestValidate(new OrderCreateDto(null!)).ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_quantity_fails(int quantity)
    {
        _validator.TestValidate(Order(new OrderItemCreateDto(1, quantity)))
            .ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void Non_positive_product_id_fails()
    {
        _validator.TestValidate(Order(new OrderItemCreateDto(0, 1)))
            .ShouldHaveValidationErrorFor("Items[0].ProductId");
    }

    [Fact]
    public void Duplicate_product_fails()
    {
        _validator.TestValidate(Order(new(7, 1), new(7, 2)))
            .ShouldHaveValidationErrorFor(x => x.Items)
            .WithErrorMessage("Each product may appear only once in an order; combine quantities instead.");
    }
}
