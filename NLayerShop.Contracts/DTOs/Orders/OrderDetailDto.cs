namespace NLayerShop.Contracts.DTOs.Orders
{
    public record OrderDetailDto(
        int Id,
        string OrderNumber,
        string Status, // Pending | Completed | Cancelled
        decimal TotalAmount,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<OrderItemDto> Items
    );
}
