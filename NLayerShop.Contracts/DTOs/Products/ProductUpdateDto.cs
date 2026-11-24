using System.Text.Json.Serialization;

namespace NLayerShop.Contracts.DTOs.Products
{
    /// <summary>
    /// PUT = full replace: all fields are required. CategoryId = null means "no category".
    /// RowVersion is the Base64 value from the last GET; a stale value results in 409 Conflict.
    /// </summary>
    public record ProductUpdateDto(
        string Name,
        [property: JsonRequired] decimal Price,
        string Description,
        [property: JsonRequired] int StockCount,
        int? CategoryId,
        string RowVersion
    );
}
