using System.Data.Common;
using Attendify.Common.Interfaces;
using Attendify.Common.FacialRecognition;
using Attendify.Common.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Attendify.IntegrationTests.Common.Infrastructure.Web;

/// <summary>
/// Host builder (services, DI and configuration) for integration tests
/// </summary>
public class WebApiTestFactory : WebApplicationFactory<IWebApiMarker>
{
    private readonly DbConnection _dbConnection;

    public WebApiTestFactory(DbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        const string TestEncryptionKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

        builder.UseSetting("ConnectionStrings:AppDb", _dbConnection.ConnectionString);
        builder.UseSetting("DataRetention:AttendanceEventDays", "1825");
        builder.UseSetting("Resend:ApiKey", "integration-test-api-key");
        builder.UseSetting("Email:FromAddress", "integration-tests@example.com");
        builder.UseSetting("Email:FromName", "Attendify Integration Tests");
        builder.UseSetting("FacialEmbedding:EncryptionKey", TestEncryptionKey);
        builder.UseSetting("Jwt:Issuer", "Attendify.IntegrationTests");
        builder.UseSetting("Jwt:Audience", "Attendify.IntegrationTests");
        builder.UseSetting("Jwt:SigningKey", "integration-test-signing-key-at-least-32-bytes");
        builder.UseSetting("Jwt:LifetimeMinutes", "15");
        builder.UseSetting("RefreshToken:LifetimeMinutes", "43200");
        builder.UseSetting("ResetPasswordToken:LifetimeMinutes", "15");
        builder.UseSetting("ResetPasswordToken:SecurityCodeHashKey", TestEncryptionKey);
        builder.UseSetting(
            "Parameters:StudentAccessCodeGenSecret",
            "integration-test-access-code-secret"
        );
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IFacialEmbeddingService>();
            services.AddSingleton<IFacialEmbeddingService, DeterministicFacialEmbeddingService>();
            services.RemoveAll<IFacialComparisonService>();
            services.AddSingleton<IFacialComparisonService, DeterministicFacialComparisonService>();
        });
    }
}

internal sealed class DeterministicFacialEmbeddingService : IFacialEmbeddingService
{
    public Task<IReadOnlyList<byte[]>> CreateEmbeddingsAsync(
        IReadOnlyList<byte[]> photos,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<byte[]>>(
            photos.Select(photo => photo.ToArray()).ToArray()
        );
    }

    public Task<byte[]> CreateEmbeddingAsync(byte[] photo, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(photo.ToArray());
    }
}

internal sealed class DeterministicFacialComparisonService : IFacialComparisonService
{
    public bool IsMatch(IReadOnlyList<byte[]> referenceEmbeddings, byte[] candidateEmbedding) =>
        referenceEmbeddings.Any(referenceEmbedding =>
            referenceEmbedding.SequenceEqual(candidateEmbedding)
        );
}
