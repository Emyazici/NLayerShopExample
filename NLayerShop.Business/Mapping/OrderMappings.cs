using System.Linq.Expressions;
using NLayerShop.Contracts.DTOs.Orders;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Business.Mapping;

// Orders are read-only here; creation rules live in OrderService.
// Status is stored as a string column, so Status.ToString() maps straight to that column.
public static class OrderMappings
{
    public static readonly Expression<Func<Order, OrderListItemDto>> ToListItemDto = o => new OrderListItemDto(
        o.Id,
        o.OrderNumber,
        o.Status.ToString(),
        o.TotalAmount,
        o.CreatedAt);

    public static readonly Expression<Func<Order, OrderDetailDto>> ToDetailDto = o => new OrderDetailDto(
        o.Id,
        o.OrderNumber,
        o.Status.ToString(),
        o.TotalAmount,
        o.CreatedAt,
        o.UpdatedAt,
        o.Items
            .OrderBy(i => i.Id)
            .Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.LineTotal))
            .ToList());
}
