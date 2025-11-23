using NLayerShop.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace NLayerShop.Domain.Entities
{
    public class Product: BaseEntity
    {
        public String Name { get; set; }
        public String Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int StockCount { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
