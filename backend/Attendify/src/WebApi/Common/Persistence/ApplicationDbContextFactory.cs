using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore.Design;

namespace Attendify.Common.Persistence;

/// <summary>
/// Creates the DbContext for EF Core tooling without starting the Web API host.
/// Runtime-only settings such as Resend credentials are not needed to build migrations.
/// </summary>
public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string DesignTimeFallbackConnectionString =
        "Host=localhost;Port=5432;Database=attendify;Username=postgres;Password=postgres";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        _ = args;

        string settingsDirectory = FindSettingsDirectory();
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environments.Development;

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(settingsDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddUserSecrets<ApplicationDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString = configuration.GetConnectionString("AppDb")
            ?? DesignTimeFallbackConnectionString;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static string FindSettingsDirectory()
    {
        for (
            DirectoryInfo? directory = new(Directory.GetCurrentDirectory());
            directory is not null;
            directory = directory.Parent
        )
        {
            if (File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
                return directory.FullName;
        }

        return AppContext.BaseDirectory;
    }
}
