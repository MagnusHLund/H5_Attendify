using Npgsql;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Attendify.Common.Persistence;
using System.Data.Common;

namespace Attendify.IntegrationTests.Common.Infrastructure.Database;

/// <summary>
/// Manages the schema and data for the database container
/// </summary>
public class TestDatabase : IAsyncDisposable
{
    private readonly PostgreSqlTestContainer _npgsqlContainer = new();
    private Respawner _checkpoint = null!;
    private string _connectionString = null!;

    /// <summary>
    /// Create and seed a database
    /// </summary>
    public async Task InitializeAsync()
    {
        await _npgsqlContainer.InitializeAsync();

        var builder = new NpgsqlConnectionStringBuilder(_npgsqlContainer.Connection!.ConnectionString)
        {
            Database = "WebApi-IntegrationTests"
        };

        _connectionString = builder.ConnectionString;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        using var dbContext = new ApplicationDbContext(options);
        await dbContext.Database.MigrateAsync();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        _checkpoint = await Respawner.CreateAsync(connection,
            new RespawnerOptions { TablesToIgnore = ["__EFMigrationsHistory"] });
    }

    public DbConnection DbConnection => new NpgsqlConnection(_connectionString);

    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _checkpoint.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        await _npgsqlContainer.DisposeAsync();
    }
}