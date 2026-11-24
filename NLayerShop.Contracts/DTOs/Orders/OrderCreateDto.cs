namespace NLayerShop.Contracts.DTOs.Orders
{
    public record OrderCreateDto(List<OrderItemCreateDto> Items);
}
