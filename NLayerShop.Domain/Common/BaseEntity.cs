using System.ComponentModel.DataAnnotations;

namespace NLayerShop.Domain.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        [Timestamp] // EF Core concurrency icin
        public byte[]? RowVersion { get; set; }
    }
}
