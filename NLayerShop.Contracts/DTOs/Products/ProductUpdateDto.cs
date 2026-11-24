namespace NLayerShop.Contracts.DTOs.Products
{
    public record ProductUpdateDto(string? Name, decimal? Price, string? Description,int? StockCount,bool? RemoveCategory,int? CategoryId);
}
