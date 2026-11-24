namespace NLayerShop.Contracts.Common
{
    /// <summary>
    /// Query string contract for GET /api/products: paging + filtering + sorting.
    /// </summary>
    public class ProductQueryRequest : PagedRequest
    {
        /// <summary>Case-insensitive (collation dependent) "Name contains" filter.</summary>
        public string? Search { get; set; }

        public int? CategoryId { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        /// <summary>true = StockCount &gt; 0, false = StockCount == 0, null = no filter.</summary>
        public bool? InStock { get; set; }

        /// <summary>name | price | createdAt (case-insensitive). Default: createdAt.</summary>
        public string? SortBy { get; set; }

        /// <summary>asc | desc (case-insensitive). Default: desc for createdAt, asc otherwise.</summary>
        public string? SortDir { get; set; }
    }
}
