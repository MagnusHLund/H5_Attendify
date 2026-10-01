using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Attendify.IntegrationTests.Common.Infrastructure.Database;
using Attendify.IntegrationTests.Common.Infrastructure.Web;

namespace Attendify.IntegrationTests.Common;

/// <summary>
/// Initializes and resets the database before and after each test. Shared across all integration tests.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class TestingDatabaseFixture : IAsyncLifetime
{
    private readonly TestDatabase _database = new();
    private WebApiTestFactory _factory = null!;
    private IServiceScopeFactory _scopeFactory = null!;

    /// <summary>
    /// Global setup for tests
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();
        _factory = new WebApiTestFactory(_database.DbConnection);
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// Setup for each test
    /// </summary>
    public async Task TestSetup()
    {
        await _database.ResetAsync();
    }

    /// <summary>
    /// Global cleanup for tests
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // Stop the web host (and its background services) before removing the database it uses.
        await _factory.DisposeAsync();
        await _database.DisposeAsync();
    }

    public HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    public IServiceScope CreateScope() => _scopeFactory.CreateScope();
}

[CollectionDefinition]
public class TestingDatabaseFixtureCollection : ICollectionFixture<TestingDatabaseFixture>;