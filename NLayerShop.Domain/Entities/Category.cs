using NLayerShop.Domain.Common;


namespace NLayerShop.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
