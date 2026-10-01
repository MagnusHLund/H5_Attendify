using System.Data.Common;
using Attendify.Common.Domain.Users;
using Attendify.Common.FacialRecognition;
using Attendify.Common.Persistence;
using Attendify.Features.Attendance.CreateAttendance;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.TestHost;
using Attendify.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
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
            services.RemoveAll<IFacialUserIdentifier>();
            services.AddScoped<IFacialUserIdentifier, DeterministicFacialUserIdentifier>();
        });
    }
}

internal sealed class DeterministicFacialEmbeddingService : IFacialEmbeddingService
{
    private static readonly byte[] Embedding = [1, 2, 3];

    public Task<IReadOnlyList<byte[]>> CreateEmbeddingsAsync(
        IReadOnlyList<byte[]> photos,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<byte[]>>(
            photos.Select(_ => Embedding.ToArray()).ToArray()
        );
    }

    public Task<byte[]> CreateEmbeddingAsync(byte[] photo, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Embedding.ToArray());
    }
}

internal sealed class DeterministicFacialUserIdentifier(ApplicationDbContext dbContext)
    : IFacialUserIdentifier
{
    public async Task<UserId> IdentifyUserAsync(
        IFormFile image,
        CancellationToken cancellationToken
    )
    {
        int? userId = await dbContext.Users
            .Where(user =>
                user.AttendanceEnabled
                && !user.IsDeleted
                && user.FacialProfile != null
                && user.FacialProfile.FacialEmbeddings.Any()
            )
            .Select(user => (int?)user.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (userId is null)
            throw new Attendify.Common.FacialRecognition.NoMatchingUserException(
                "No active student has a face profile."
            );

        return UserId.From(userId.Value);
    }
}
