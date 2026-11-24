using System.Text.Json.Serialization;

namespace NLayerShop.Contracts.DTOs.Products
{
    /// <summary>
    /// Price and StockCount are value types: without JsonRequired a missing field would silently
    /// bind as 0 (a free product / zero stock) instead of returning 400.
    /// </summary>
    public record ProductCreateDto(
        string Name,
        [property: JsonRequired] decimal Price,
        string Description,
        [property: JsonRequired] int StockCount,
        int? CategoryId
    );
}
