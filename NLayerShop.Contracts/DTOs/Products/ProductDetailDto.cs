namespace NLayerShop.Contracts.DTOs.Products
{
    public record ProductDetailDto(
        int Id,
        string Name,
        decimal Price,
        string Description,
        int StockCount,
        int? CategoryId,
        string? CategoryName,
        string RowVersion // Base64; PUT isteginde aynen geri gonderilmeli (optimistic concurrency)
    );
}
