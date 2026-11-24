using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Soft delete: pasif kayitlar tum sorgulardan otomatik olarak dislanir.
        builder.HasQueryFilter(p => p.IsActive);

        // Lengths mirror ProductCreate/UpdateDtoValidator.
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .HasPrecision(18, 2); // SQL: decimal(18,2)
    }
}
