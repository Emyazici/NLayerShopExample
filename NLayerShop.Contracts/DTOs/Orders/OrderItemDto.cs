namespace NLayerShop.Contracts.DTOs.Orders
{
    // ProductName/UnitPrice siparis anindaki snapshot'tir; urun sonradan degisse de sabit kalir.
    public record OrderItemDto(
        int ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal
    );
}
