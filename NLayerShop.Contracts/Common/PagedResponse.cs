namespace NLayerShop.Contracts.Common
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int Size { get; init; }
        public int TotalCount { get; init; }

        public int TotalPages => Size > 0 ? (int)Math.Ceiling(TotalCount / (double)Size) : 0;
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
