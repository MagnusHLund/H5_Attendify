using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Common.Persistence;
using Attendify.Features.Auth.CompletePasswordReset;
using Attendify.Features.Auth.LoginWithPassword;
using Attendify.Features.Auth.VerifyPasswordReset;
using Attendify.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace Attendify.IntegrationTests;

public sealed class PasswordResetEndpointTests(TestingDatabaseFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string SecurityCodeHashKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
    private const string SecurityCode = "A23456789";

    [Fact]
    public async Task RequestPasswordReset_ForUnknownAccountDoesNotRevealWhetherAccountExists()
    {
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/password-reset/request",
            new { Email = "unknown@example.com" },
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task VerifyAndCompletePasswordReset_ConsumeCodeAndReplacePassword()
    {
        using HttpClient previousSession = await CreateAuthenticatedStudentClientAsync();
        ApplicationDbContext dbContext = GetService<ApplicationDbContext>();
        User user = await dbContext.Users.SingleAsync(cancellationToken: CancellationToken);
        await AddResetTokenAsync(user.Id);
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage invalidCodeResponse = await client.PostAsJsonAsync(
            "/api/auth/password-reset/verify",
            new VerifyPasswordResetRequest(user.Email, "A23456780"),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.BadRequest, invalidCodeResponse.StatusCode);

        using HttpResponseMessage verifyResponse = await client.PostAsJsonAsync(
            "/api/auth/password-reset/verify",
            new VerifyPasswordResetRequest(user.Email, SecurityCode),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, verifyResponse.StatusCode);

        using HttpResponseMessage completeResponse = await client.PostAsJsonAsync(
            "/api/auth/password-reset/complete",
            new CompletePasswordResetRequest(user.Email, SecurityCode, "new password 123"),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, completeResponse.StatusCode);

        using HttpResponseMessage usedCodeResponse = await client.PostAsJsonAsync(
            "/api/auth/password-reset/verify",
            new VerifyPasswordResetRequest(user.Email, SecurityCode),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.BadRequest, usedCodeResponse.StatusCode);

        using HttpResponseMessage oldPasswordResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginWithPasswordRequest
            {
                Email = user.Email,
                Password = "correct horse battery"
            },
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.BadRequest, oldPasswordResponse.StatusCode);

        using HttpResponseMessage newPasswordResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginWithPasswordRequest
            {
                Email = user.Email,
                Password = "new password 123"
            },
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, newPasswordResponse.StatusCode);
        using HttpResponseMessage previousSessionResponse = await previousSession.GetAsync(
            "/api/auth/me",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.Unauthorized, previousSessionResponse.StatusCode);
    }

    [Fact]
    public async Task VerifyPasswordReset_LocksAfterThreeFailedAttempts()
    {
        using HttpClient seedClient = await CreateAuthenticatedStudentClientAsync();
        User user = await GetService<ApplicationDbContext>()
            .Users.SingleAsync(cancellationToken: CancellationToken);
        await AddResetTokenAsync(user.Id);
        using HttpClient client = GetAnonymousClient();

        for (int attempt = 0; attempt < PasswordResetToken.MaxFailedAttempts; attempt++)
        {
            using HttpResponseMessage invalidCodeResponse = await client.PostAsJsonAsync(
                "/api/auth/password-reset/verify",
                new VerifyPasswordResetRequest(user.Email, "A23456780"),
                CancellationToken
            );
            Assert.Equal(HttpStatusCode.BadRequest, invalidCodeResponse.StatusCode);
        }

        using HttpResponseMessage lockedCodeResponse = await client.PostAsJsonAsync(
            "/api/auth/password-reset/verify",
            new VerifyPasswordResetRequest(user.Email, SecurityCode),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.BadRequest, lockedCodeResponse.StatusCode);
    }

    [Fact]
    public async Task VerifyPasswordReset_RejectsExpiredCode()
    {
        using HttpClient seedClient = await CreateAuthenticatedStudentClientAsync();
        User user = await GetService<ApplicationDbContext>()
            .Users.SingleAsync(cancellationToken: CancellationToken);
        await AddResetTokenAsync(user.Id, TimeSpan.FromMinutes(-1));
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/password-reset/verify",
            new VerifyPasswordResetRequest(user.Email, SecurityCode),
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task AddResetTokenAsync(int userId, TimeSpan? validityPeriod = null)
    {
        byte[] hash = HMACSHA256.HashData(
            Convert.FromBase64String(SecurityCodeHashKey),
            Encoding.UTF8.GetBytes($"{userId}:{SecurityCode}")
        );
        PasswordResetToken token = PasswordResetToken.Create(
            userId,
            hash,
            DateTimeOffset.UtcNow.Add(validityPeriod ?? TimeSpan.FromMinutes(10))
        );
        token.SetCreated(TimeProvider.System, null);
        await AddAsync(token);
    }
}
