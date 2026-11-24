namespace NLayerShop.Contracts.DTOs.Orders
{
    public record OrderListItemDto(
        int Id,
        string OrderNumber,
        string Status,
        decimal TotalAmount,
        DateTime CreatedAt
    );
}
