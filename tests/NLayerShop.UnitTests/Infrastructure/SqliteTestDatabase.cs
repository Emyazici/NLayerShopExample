using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NLayerShop.Data.Context;

namespace NLayerShop.UnitTests.Infrastructure;

/// <summary>
/// One private in-memory SQLite database per instance. The connection is kept open for the
/// lifetime of the instance because an in-memory SQLite database disappears when it closes.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    /// <summary>Creates a new context over the same database (use a fresh one for assertions).</summary>
    public AppDbContext CreateContext() => new SqliteTestDbContext(_options);

    public void Dispose() => _connection.Dispose();
}
