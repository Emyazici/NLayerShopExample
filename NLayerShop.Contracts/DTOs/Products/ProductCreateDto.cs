namespace NLayerShop.Contracts.DTOs.Products
{
    public record ProductCreateDto(string Name, decimal Price, string Description,int StockCount, int? CategoryId);

}
