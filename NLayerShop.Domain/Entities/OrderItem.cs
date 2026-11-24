using NLayerShop.Domain.Common;

namespace NLayerShop.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }

        // Intentionally no Product navigation: the order keeps its own snapshot (name/price),
        // so a soft-deleted or repriced product never changes or hides order history.
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty; // snapshot
        public decimal UnitPrice { get; set; }                 // snapshot
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
