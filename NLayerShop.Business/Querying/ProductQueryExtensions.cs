using NLayerShop.Contracts.Common;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Business.Querying
{
    /// <summary>
    /// Whitelisted sort keys and directions for product listing (shared by validator and query code).
    /// </summary>
    public static class ProductSortOptions
    {
        public const string Name = "name";
        public const string Price = "price";
        public const string CreatedAt = "createdAt";

        public const string Asc = "asc";
        public const string Desc = "desc";

        public static readonly IReadOnlyCollection<string> SortFields = new[] { Name, Price, CreatedAt };
        public static readonly IReadOnlyCollection<string> SortDirections = new[] { Asc, Desc };

        public static bool IsValidSortField(string? value) =>
            value is not null && SortFields.Contains(value, StringComparer.OrdinalIgnoreCase);

        public static bool IsValidSortDirection(string? value) =>
            value is not null && SortDirections.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Composable IQueryable filters/sorting; everything here is translated to SQL by EF Core.
    /// </summary>
    public static class ProductQueryExtensions
    {
        public static IQueryable<Product> ApplyFilters(this IQueryable<Product> query, ProductQueryRequest req)
        {
            if (!string.IsNullOrWhiteSpace(req.Search))
            {
                var search = req.Search.Trim();
                query = query.Where(p => p.Name.Contains(search));
            }

            if (req.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == req.CategoryId.Value);

            if (req.MinPrice.HasValue)
                query = query.Where(p => p.Price >= req.MinPrice.Value);

            if (req.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= req.MaxPrice.Value);

            if (req.InStock.HasValue)
            {
                query = req.InStock.Value
                    ? query.Where(p => p.StockCount > 0)
                    : query.Where(p => p.StockCount == 0);
            }

            return query;
        }

        /// <summary>
        /// Applies the requested ordering with Id as tie-breaker so paging is deterministic.
        /// Unknown values fall back to the default (validator rejects them before we get here).
        /// </summary>
        public static IOrderedQueryable<Product> ApplySorting(this IQueryable<Product> query, ProductQueryRequest req)
        {
            var sortBy = string.IsNullOrWhiteSpace(req.SortBy) ? ProductSortOptions.CreatedAt : req.SortBy.Trim();

            var descending = string.IsNullOrWhiteSpace(req.SortDir)
                ? string.Equals(sortBy, ProductSortOptions.CreatedAt, StringComparison.OrdinalIgnoreCase)
                : string.Equals(req.SortDir.Trim(), ProductSortOptions.Desc, StringComparison.OrdinalIgnoreCase);

            IOrderedQueryable<Product> ordered = sortBy.ToLowerInvariant() switch
            {
                ProductSortOptions.Name => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                ProductSortOptions.Price => descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                _ => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };

            return descending ? ordered.ThenByDescending(p => p.Id) : ordered.ThenBy(p => p.Id);
        }
    }
}
