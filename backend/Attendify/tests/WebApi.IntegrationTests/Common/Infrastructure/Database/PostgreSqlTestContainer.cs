using Npgsql;
using Polly;
using Testcontainers.PostgreSql;

namespace Attendify.IntegrationTests.Common.Infrastructure.Database;

/// <summary>
/// Wrapper for PostgreSqlTest container
/// </summary>
public class PostgreSqlTestContainer : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18.3-bookworm")
        .WithName($"WebApi-IntegrationTests-{Guid.NewGuid()}")
        .WithPassword("Password123")
        .WithPortBinding(5432, true)
        .WithAutoRemove(true)
        .Build();

    private const int MaxRetries = 5;

    public NpgsqlConnection? Connection { get; private set; }

    public async Task InitializeAsync()
    {
        await StartWithRetry();
        Connection = new NpgsqlConnection(_container.GetConnectionString());
    }

    private async Task StartWithRetry()
    {
        // NOTE: For some reason the container sometimes fails to start up.  Add in a retry to protect against this
        var policy = Policy.Handle<InvalidOperationException>()
            .WaitAndRetryAsync(MaxRetries, _ => TimeSpan.FromSeconds(5));

        await policy.ExecuteAsync(async () => { await _container.StartAsync(); });
    }

    public async ValueTask DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}