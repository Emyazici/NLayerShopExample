namespace NLayerShop.Contracts.Common
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int Size { get; init; }
        public int TotalCount { get; init; }
    }
}
