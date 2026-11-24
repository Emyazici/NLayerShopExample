using NLayerShop.Contracts.Common;

namespace NLayerShop.Contracts.DTOs.Orders
{
    public class OrderQueryRequest : PagedRequest
    {
        // Opsiyonel filtre: Pending | Completed | Cancelled (buyuk/kucuk harf duyarsiz).
        public string? Status { get; set; }
    }
}
