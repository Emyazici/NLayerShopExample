using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLayerShop.Data.Context;
using Testcontainers.MsSql;

namespace NLayerShop.IntegrationTests.Infrastructure;

/// <summary>
/// Hosts the real API in memory (environment "Testing") against a throw-away SQL Server container.
/// One instance is shared by every test in the "Api" collection, so the container starts once per run.
/// Tests isolate themselves with unique names instead of resetting the database.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";

    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder(SqlServerImage).Build();

    private string _connectionString = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
    }

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        // Use a dedicated database instead of the container's default "master".
        _connectionString = new SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
        {
            InitialCatalog = "NLayerShopIntegrationTests"
        }.ConnectionString;

        // Accessing Services builds the host; then apply the real migrations (same path as production).
        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _sqlServer.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Api";
}
