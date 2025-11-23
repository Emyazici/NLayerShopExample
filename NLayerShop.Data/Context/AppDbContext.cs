using Microsoft.EntityFrameworkCore;
using NLayerShop.Domain.Entities;

namespace NLayerShop.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2); // SQL: decimal(18,2)

        modelBuilder.Entity<Category>()
            .HasMany(c=>c.Products)
            .WithOne(p=>p.Category)
            .HasForeignKey(p=>p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
