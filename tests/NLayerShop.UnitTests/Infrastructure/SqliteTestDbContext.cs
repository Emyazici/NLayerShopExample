using Microsoft.EntityFrameworkCore;
using NLayerShop.Data.Context;
using NLayerShop.Domain.Common;

namespace NLayerShop.UnitTests.Infrastructure;

/// <summary>
/// AppDbContext for SQLite-backed tests. SQL Server generates [Timestamp] rowversion values
/// automatically; SQLite does not, so every BaseEntity.RowVersion gets a random 8-byte default.
/// The value stays constant on UPDATE, which is enough for the optimistic-concurrency WHERE clause
/// to match in these single-user tests.
/// </summary>
public sealed class SqliteTestDbContext : AppDbContext
{
    public SqliteTestDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var auditedTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType))
            .Select(t => t.ClrType)
            .ToList();

        foreach (var clrType in auditedTypes)
        {
            modelBuilder.Entity(clrType)
                .Property(nameof(BaseEntity.RowVersion))
                .HasDefaultValueSql("randomblob(8)");
        }
    }
}
