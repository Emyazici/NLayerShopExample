namespace NLayerShop.Contracts.DTOs.Products
{
    public record ProductDetailDto(
        int Id,
        string Name,
        decimal Price,
        string Description,
        int StockCount,
        int? CategoryId,
        string? CategoryName
    );
}
