using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using NLayerShop.Business.Exceptions;
using NLayerShop.Business.Services;
using NLayerShop.Business.Validation;
using NLayerShop.Contracts.DTOs.Orders;
using NLayerShop.Data.Context;
using NLayerShop.Data.UnitOfWork;
using NLayerShop.Domain.Entities;
using NLayerShop.Domain.Enums;
using NLayerShop.UnitTests.Infrastructure;

namespace NLayerShop.UnitTests.Services;

/// <summary>
/// Business-rule tests for OrderService against the real AppDbContext, UnitOfWork and AutoMapper
/// profile on in-memory SQLite (no IQueryable mocking).
/// </summary>
public sealed class OrderServiceTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();
    private readonly List<AppDbContext> _contexts = new();

    public void Dispose()
    {
        foreach (var context in _contexts)
            context.Dispose();
        _db.Dispose();
    }

    /// <summary>
    /// A service over a fresh DbContext, like one HTTP request scope. Avoids stale tracked
    /// entities when a test changes data between service calls.
    /// </summary>
    private OrderService Sut()
    {
        var context = _db.CreateContext();
        _contexts.Add(context);

        return new OrderService(
            new UnitOfWork(context),
            TestMapper.Mapper,
            new OrderCreateDtoValidator(),
            new OrderQueryRequestValidator());
    }

    [Fact]
    public async Task CreateAsync_decrements_stock_and_snapshots_name_and_price()
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 10);
        var case_ = await SeedProductAsync("Case", price: 15.50m, stock: 3);

        var order = await Sut().CreateAsync(Order((phone.Id, 2), (case_.Id, 3)), CancellationToken.None);

        order.Status.Should().Be(nameof(OrderStatus.Pending));
        order.OrderNumber.Should().StartWith("ORD-");
        order.TotalAmount.Should().Be(2 * 100m + 3 * 15.50m);
        order.Items.Should().BeEquivalentTo(new[]
        {
            new OrderItemDto(phone.Id, "Phone", 100m, 2, 200m),
            new OrderItemDto(case_.Id, "Case", 15.50m, 3, 46.50m)
        }, o => o.WithStrictOrdering());

        (await StockOfAsync(phone.Id)).Should().Be(8);
        (await StockOfAsync(case_.Id)).Should().Be(0);
    }

    [Fact]
    public async Task Order_items_keep_snapshot_after_product_is_repriced()
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 10);
        var order = await Sut().CreateAsync(Order((phone.Id, 1)), CancellationToken.None);

        await using (var ctx = _db.CreateContext())
        {
            var product = await ctx.Products.SingleAsync(p => p.Id == phone.Id);
            product.Name = "Phone v2";
            product.Price = 250m;
            await ctx.SaveChangesAsync();
        }

        var reloaded = await Sut().GetByIdAsync(order.Id, CancellationToken.None);

        reloaded.Items.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new OrderItemDto(phone.Id, "Phone", 100m, 1, 100m));
        reloaded.TotalAmount.Should().Be(100m);
    }

    [Fact]
    public async Task CreateAsync_with_insufficient_stock_throws_and_changes_nothing()
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 2);

        var act = () => Sut().CreateAsync(Order((phone.Id, 3)), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*Insufficient stock*requested 3, available 2*");
        (await StockOfAsync(phone.Id)).Should().Be(2);
        (await OrderCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_checks_all_lines_before_touching_any_stock()
    {
        var plenty = await SeedProductAsync("Plenty", price: 1m, stock: 100);
        var scarce = await SeedProductAsync("Scarce", price: 1m, stock: 1);

        var act = () => Sut().CreateAsync(Order((plenty.Id, 5), (scarce.Id, 2)), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        (await StockOfAsync(plenty.Id)).Should().Be(100);
        (await StockOfAsync(scarce.Id)).Should().Be(1);
        (await OrderCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_with_duplicate_product_is_rejected_and_changes_nothing()
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 10);

        var act = () => Sut().CreateAsync(Order((phone.Id, 1), (phone.Id, 2)), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*only once*");
        (await StockOfAsync(phone.Id)).Should().Be(10);
        (await OrderCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_with_unknown_product_throws_not_found()
    {
        var act = () => Sut().CreateAsync(Order((12345, 1)), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*12345*");
    }

    [Fact]
    public async Task CreateAsync_with_soft_deleted_product_throws_not_found()
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 10, isActive: false);

        var act = () => Sut().CreateAsync(Order((phone.Id, 1)), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_validation_failure_never_touches_the_database()
    {
        var validator = new Mock<IValidator<OrderCreateDto>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<OrderCreateDto>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new[] { new ValidationFailure("Items", "invalid") }));
        var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

        var sut = new OrderService(uow.Object, TestMapper.Mapper, validator.Object, new OrderQueryRequestValidator());

        var act = () => sut.CreateAsync(Order((1, 1)), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        validator.Verify(
            v => v.ValidateAsync(It.IsAny<ValidationContext<OrderCreateDto>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CancelAsync_on_pending_order_restores_stock()
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 10);
        var order = await Sut().CreateAsync(Order((phone.Id, 4)), CancellationToken.None);
        (await StockOfAsync(phone.Id)).Should().Be(6);

        await Sut().CancelAsync(order.Id, CancellationToken.None);

        (await StatusOfAsync(order.Id)).Should().Be(OrderStatus.Cancelled);
        (await StockOfAsync(phone.Id)).Should().Be(10);
    }

    [Fact]
    public async Task CancelAsync_restores_stock_even_if_product_was_soft_deleted()
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 10);
        var order = await Sut().CreateAsync(Order((phone.Id, 4)), CancellationToken.None);

        await using (var ctx = _db.CreateContext())
        {
            var product = await ctx.Products.SingleAsync(p => p.Id == phone.Id);
            product.IsActive = false;
            await ctx.SaveChangesAsync();
        }

        await Sut().CancelAsync(order.Id, CancellationToken.None);

        (await StockOfAsync(phone.Id)).Should().Be(10);
    }

    [Theory]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task CancelAsync_only_allows_pending_orders(OrderStatus status)
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 10);
        var order = await Sut().CreateAsync(Order((phone.Id, 4)), CancellationToken.None);
        await SetStatusAsync(order.Id, status);

        var act = () => Sut().CancelAsync(order.Id, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Only pending orders can be cancelled*");
        (await StatusOfAsync(order.Id)).Should().Be(status);
        (await StockOfAsync(phone.Id)).Should().Be(6, "a rejected cancel must not return stock");
    }

    [Fact]
    public async Task CompleteAsync_on_pending_order_marks_it_completed_without_touching_stock()
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 10);
        var order = await Sut().CreateAsync(Order((phone.Id, 4)), CancellationToken.None);

        await Sut().CompleteAsync(order.Id, CancellationToken.None);

        (await StatusOfAsync(order.Id)).Should().Be(OrderStatus.Completed);
        (await StockOfAsync(phone.Id)).Should().Be(6);
    }

    [Theory]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task CompleteAsync_only_allows_pending_orders(OrderStatus status)
    {
        var phone = await SeedProductAsync("Phone", price: 100m, stock: 10);
        var order = await Sut().CreateAsync(Order((phone.Id, 1)), CancellationToken.None);
        await SetStatusAsync(order.Id, status);

        var act = () => Sut().CompleteAsync(order.Id, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("Only pending orders can be completed*");
        (await StatusOfAsync(order.Id)).Should().Be(status);
    }

    [Fact]
    public async Task Cancel_and_complete_on_missing_order_throw_not_found()
    {
        var cancel = () => Sut().CancelAsync(999, CancellationToken.None);
        var complete = () => Sut().CompleteAsync(999, CancellationToken.None);

        await cancel.Should().ThrowAsync<NotFoundException>();
        await complete.Should().ThrowAsync<NotFoundException>();
    }

    // ---------------- helpers ----------------

    private static OrderCreateDto Order(params (int ProductId, int Quantity)[] lines) =>
        new(lines.Select(l => new OrderItemCreateDto(l.ProductId, l.Quantity)).ToList());

    private async Task<Product> SeedProductAsync(string name, decimal price, int stock, bool isActive = true)
    {
        await using var ctx = _db.CreateContext();
        var product = new Product { Name = name, Description = name, Price = price, StockCount = stock };
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();

        if (!isActive)
        {
            product.IsActive = false;
            await ctx.SaveChangesAsync();
        }

        return product;
    }

    private async Task SetStatusAsync(int orderId, OrderStatus status)
    {
        await using var ctx = _db.CreateContext();
        var order = await ctx.Orders.SingleAsync(o => o.Id == orderId);
        order.Status = status;
        await ctx.SaveChangesAsync();
    }

    private async Task<int> StockOfAsync(int productId)
    {
        await using var ctx = _db.CreateContext();
        return await ctx.Products.IgnoreQueryFilters()
            .Where(p => p.Id == productId)
            .Select(p => p.StockCount)
            .SingleAsync();
    }

    private async Task<OrderStatus> StatusOfAsync(int orderId)
    {
        await using var ctx = _db.CreateContext();
        return await ctx.Orders.Where(o => o.Id == orderId).Select(o => o.Status).SingleAsync();
    }

    private async Task<int> OrderCountAsync()
    {
        await using var ctx = _db.CreateContext();
        return await ctx.Orders.CountAsync();
    }
}
