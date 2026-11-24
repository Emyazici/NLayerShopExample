namespace NLayerShop.Contracts.DTOs.Products
{
    /// <summary>
    /// PUT = full replace: all fields are required. CategoryId = null means "no category".
    /// RowVersion is the Base64 value from the last GET; a stale value results in 409 Conflict.
    /// </summary>
    public record ProductUpdateDto(
        string Name,
        decimal Price,
        string Description,
        int StockCount,
        int? CategoryId,
        string RowVersion
    );
}
