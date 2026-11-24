using System.Net;
using FluentAssertions;
using NLayerShop.Contracts.DTOs.Orders;
using NLayerShop.IntegrationTests.Infrastructure;

namespace NLayerShop.IntegrationTests;

public class OrdersApiTests : ApiTestBase
{
    public OrdersApiTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Order_decrements_stock_and_cancel_restores_it()
    {
        var phone = await CreateProductAsync(price: 100m, stock: 10);
        var cable = await CreateProductAsync(price: 7.25m, stock: 4);

        var order = await CreateOrderAsync((phone.Id, 3), (cable.Id, 4));

        order.Status.Should().Be("Pending");
        order.TotalAmount.Should().Be(3 * 100m + 4 * 7.25m);
        order.Items.Should().HaveCount(2);
        order.Items.Should().ContainSingle(i => i.ProductId == phone.Id)
            .Which.Should().Match<OrderItemDto>(i =>
                i.ProductName == phone.Name && i.UnitPrice == 100m && i.Quantity == 3 && i.LineTotal == 300m);
        (await GetProductAsync(phone.Id)).StockCount.Should().Be(7);
        (await GetProductAsync(cable.Id)).StockCount.Should().Be(0);

        var cancel = await Client.PostAsync($"/api/orders/{order.Id}/cancel", null);

        cancel.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetOrderAsync(order.Id)).Status.Should().Be("Cancelled");
        (await GetProductAsync(phone.Id)).StockCount.Should().Be(10);
        (await GetProductAsync(cable.Id)).StockCount.Should().Be(4);
    }

    [Fact]
    public async Task Insufficient_stock_returns_422_and_leaves_stock_untouched()
    {
        var plenty = await CreateProductAsync(stock: 50);
        var scarce = await CreateProductAsync(stock: 2);

        var response = await PostOrderAsync((plenty.Id, 5), (scarce.Id, 3));

        var problem = await ShouldBeProblemAsync(response, HttpStatusCode.UnprocessableEntity);
        problem.Detail.Should().Contain("Insufficient stock");
        (await GetProductAsync(plenty.Id)).StockCount.Should().Be(50);
        (await GetProductAsync(scarce.Id)).StockCount.Should().Be(2);
    }

    [Fact]
    public async Task Completed_order_cannot_be_cancelled()
    {
        var product = await CreateProductAsync(stock: 5);
        var order = await CreateOrderAsync((product.Id, 2));

        var complete = await Client.PostAsync($"/api/orders/{order.Id}/complete", null);
        complete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cancel = await Client.PostAsync($"/api/orders/{order.Id}/cancel", null);

        var problem = await ShouldBeProblemAsync(cancel, HttpStatusCode.UnprocessableEntity);
        problem.Detail.Should().Contain("Only pending orders can be cancelled");
        (await GetOrderAsync(order.Id)).Status.Should().Be("Completed");
        (await GetProductAsync(product.Id)).StockCount.Should().Be(3, "stock is not returned for a completed order");
    }

    [Fact]
    public async Task Cancelled_order_cannot_be_completed_or_cancelled_again()
    {
        var product = await CreateProductAsync(stock: 5);
        var order = await CreateOrderAsync((product.Id, 2));
        (await Client.PostAsync($"/api/orders/{order.Id}/cancel", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        await ShouldBeProblemAsync(await Client.PostAsync($"/api/orders/{order.Id}/complete", null),
            HttpStatusCode.UnprocessableEntity);
        await ShouldBeProblemAsync(await Client.PostAsync($"/api/orders/{order.Id}/cancel", null),
            HttpStatusCode.UnprocessableEntity);
        (await GetProductAsync(product.Id)).StockCount.Should().Be(5, "stock must be restored exactly once");
    }

    [Fact]
    public async Task Invalid_orders_return_400()
    {
        var product = await CreateProductAsync();

        var empty = await ShouldBeValidationProblemAsync(await PostOrderAsync());
        empty.Errors.Keys.Should().Contain(k => k.Equals("Items", StringComparison.OrdinalIgnoreCase));

        var zeroQuantity = await ShouldBeValidationProblemAsync(await PostOrderAsync((product.Id, 0)));
        zeroQuantity.Errors.Keys.Should().Contain(k => k.EndsWith("Quantity", StringComparison.OrdinalIgnoreCase));

        var duplicate = await ShouldBeValidationProblemAsync(await PostOrderAsync((product.Id, 1), (product.Id, 1)));
        duplicate.Errors.Values.SelectMany(v => v).Should().Contain(m => m.Contains("only once"));

        (await GetProductAsync(product.Id)).StockCount.Should().Be(product.StockCount);
    }

    [Fact]
    public async Task Unknown_or_soft_deleted_product_and_unknown_order_return_404()
    {
        await ShouldBeProblemAsync(await PostOrderAsync((987654321, 1)), HttpStatusCode.NotFound);

        var product = await CreateProductAsync();
        (await Client.DeleteAsync($"/api/products/{product.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        await ShouldBeProblemAsync(await PostOrderAsync((product.Id, 1)), HttpStatusCode.NotFound);

        await ShouldBeProblemAsync(await Client.GetAsync("/api/orders/987654321"), HttpStatusCode.NotFound);
        await ShouldBeProblemAsync(await Client.PostAsync("/api/orders/987654321/cancel", null), HttpStatusCode.NotFound);
    }
}
