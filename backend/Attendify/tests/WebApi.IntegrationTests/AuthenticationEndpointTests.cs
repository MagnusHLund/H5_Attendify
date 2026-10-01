using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Attendify.Common.Persistence;
using Attendify.Features.Auth.LoginWithAccessCode;
using Attendify.Features.Auth.LoginWithPassword;
using Attendify.Features.Settings.StudentAccessCode;
using Attendify.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace Attendify.IntegrationTests;

public sealed class AuthenticationEndpointTests(TestingDatabaseFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task LoginWithPassword_AuthenticatesStudentAndReturnsCurrentUser()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage response = await client.GetAsync("/api/auth/me", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument currentUser = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken
        );
        Assert.Equal("student", currentUser.RootElement.GetProperty("userType").GetString());
        Assert.Equal("student-123", currentUser.RootElement.GetProperty("studentId").GetString());
    }

    [Fact]
    public async Task LoginWithPassword_RejectsInvalidCredentials()
    {
        using HttpClient client = GetAnonymousClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginWithPasswordRequest
            {
                Email = "missing@example.com",
                Password = "correct horse battery"
            },
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task GetCurrentUser_RequiresAuthentication()
    {
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage response = await client.GetAsync("/api/auth/me", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshWithoutCookie_ReturnsUnauthorized()
    {
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage response = await client.PostAsync(
            "/api/auth/refresh",
            content: null,
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshWithInvalidCookie_ReturnsUnauthorizedAndClearsCookies()
    {
        using HttpClient client = GetAnonymousClient();
        client.DefaultRequestHeaders.Add("Cookie", "RefreshToken=not-a-valid-token");

        using HttpResponseMessage response = await client.PostAsync(
            "/api/auth/refresh",
            content: null,
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            cookie => cookie.StartsWith("RefreshToken=;", StringComparison.Ordinal)
        );
    }

    [Fact]
    public async Task LogoutWithoutCookie_SucceedsAndClearsSession()
    {
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage response = await client.PostAsync(
            "/api/auth/logout",
            content: null,
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), cookie =>
            cookie.StartsWith("AccessToken=", StringComparison.Ordinal));
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), cookie =>
            cookie.StartsWith("RefreshToken=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RefreshThenLogout_RotatesAndRevokesSession()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage refreshResponse = await client.PostAsync(
            "/api/auth/refresh",
            content: null,
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, refreshResponse.StatusCode);
        using HttpResponseMessage currentUserResponse = await client.GetAsync(
            "/api/auth/me",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.OK, currentUserResponse.StatusCode);

        using HttpResponseMessage logoutResponse = await client.PostAsync(
            "/api/auth/logout",
            content: null,
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        using HttpResponseMessage revokedSessionResponse = await client.GetAsync(
            "/api/auth/me",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.Unauthorized, revokedSessionResponse.StatusCode);
    }

    [Fact]
    public async Task StudentAccessCode_AllowsAdministratorLoginWithoutRefreshToken()
    {
        using HttpClient studentClient = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage accessCodeResponse = await studentClient.GetAsync(
            "/api/settings/student-access-code",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.OK, accessCodeResponse.StatusCode);
        StudentAccessCodeResponse? accessCode = await accessCodeResponse.Content
            .ReadFromJsonAsync<StudentAccessCodeResponse>(CancellationToken);
        Assert.NotNull(accessCode);

        using HttpResponseMessage repeatedCodeResponse = await studentClient.GetAsync(
            "/api/settings/student-access-code",
            CancellationToken
        );
        StudentAccessCodeResponse? repeatedCode = await repeatedCodeResponse.Content
            .ReadFromJsonAsync<StudentAccessCodeResponse>(CancellationToken);
        Assert.NotNull(repeatedCode);
        Assert.Equal(accessCode.StudentAccessCode, repeatedCode.StudentAccessCode);

        using HttpClient administratorClient = GetAnonymousClient();
        using HttpResponseMessage loginResponse = await administratorClient.PostAsJsonAsync(
            "/api/auth/login/access-code",
            new LoginWithAccessCodeRequest(accessCode.StudentAccessCode),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
        Assert.Contains(
            loginResponse.Headers.GetValues("Set-Cookie"),
            cookie => cookie.StartsWith("RefreshToken=;", StringComparison.Ordinal)
        );

        using HttpResponseMessage currentUserResponse = await administratorClient.GetAsync(
            "/api/auth/me",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.OK, currentUserResponse.StatusCode);
        using JsonDocument currentUser = await JsonDocument.ParseAsync(
            await currentUserResponse.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken
        );
        Assert.Equal(
            "school_administrator",
            currentUser.RootElement.GetProperty("userType").GetString()
        );

        using HttpResponseMessage refreshResponse = await administratorClient.PostAsync(
            "/api/auth/refresh",
            content: null,
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);
    }

    [Fact]
    public async Task LoginWithAccessCode_RejectsExpiredCode()
    {
        using HttpClient studentClient = await CreateAuthenticatedStudentClientAsync();
        using HttpResponseMessage codeResponse = await studentClient.GetAsync(
            "/api/settings/student-access-code",
            CancellationToken
        );
        StudentAccessCodeResponse? accessCode = await codeResponse.Content
            .ReadFromJsonAsync<StudentAccessCodeResponse>(CancellationToken);
        Assert.NotNull(accessCode);

        ApplicationDbContext dbContext = GetService<ApplicationDbContext>();
        Attendify.Common.Domain.Authentication.StudentAccessCode storedCode = await dbContext
            .StudentAccessCodes.SingleAsync(cancellationToken: CancellationToken);
        storedCode.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        await dbContext.SaveChangesAsync(CancellationToken);

        using HttpClient administratorClient = GetAnonymousClient();
        using HttpResponseMessage loginResponse = await administratorClient.PostAsJsonAsync(
            "/api/auth/login/access-code",
            new LoginWithAccessCodeRequest(accessCode.StudentAccessCode),
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    private async Task<HttpResponseMessage> SendDeleteAccountAsync(HttpClient client)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/settings/account")
        {
            Content = JsonContent.Create(new { CurrentPassword = "password123" })
        };
        return await client.SendAsync(request, CancellationToken);
    }

    [Fact]
    public async Task LoginWithPassword_NormalizesEmailAndRejectsIncorrectPassword()
    {
        const string email = "student@example.com";
        using HttpClient setupClient = await CreateAuthenticatedStudentClientAsync(email);
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage incorrectPasswordResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginWithPasswordRequest { Email = email, Password = "wrong password" },
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.BadRequest, incorrectPasswordResponse.StatusCode);

        using HttpResponseMessage normalizedEmailResponse = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginWithPasswordRequest
            {
                Email = "  STUDENT@EXAMPLE.COM ",
                Password = "correct horse battery"
            },
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, normalizedEmailResponse.StatusCode);
        using HttpResponseMessage currentUserResponse = await client.GetAsync(
            "/api/auth/me",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.OK, currentUserResponse.StatusCode);
    }

    [Fact]
    public async Task LoginWithAccessCode_RejectsUnknownCode()
    {
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/login/access-code",
            new LoginWithAccessCodeRequest("A23456789"),
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task StudentOnlyEndpoints_RejectAdministratorAccessCodeSession()
    {
        using HttpClient studentClient = await CreateAuthenticatedStudentClientAsync();
        using HttpResponseMessage codeResponse = await studentClient.GetAsync(
            "/api/settings/student-access-code",
            CancellationToken
        );
        StudentAccessCodeResponse? accessCode = await codeResponse.Content
            .ReadFromJsonAsync<StudentAccessCodeResponse>(CancellationToken);
        Assert.NotNull(accessCode);

        using HttpClient administratorClient = GetAnonymousClient();
        using HttpResponseMessage loginResponse = await administratorClient.PostAsJsonAsync(
            "/api/auth/login/access-code",
            new LoginWithAccessCodeRequest(accessCode.StudentAccessCode),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);

        using HttpResponseMessage preferenceResponse = await administratorClient.GetAsync(
            "/api/settings/attendance-preference",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.Forbidden, preferenceResponse.StatusCode);

        using HttpResponseMessage exportResponse = await administratorClient.GetAsync(
            "/api/settings/personal-data",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.Forbidden, exportResponse.StatusCode);

        using HttpResponseMessage updatePreferenceResponse = await administratorClient.PatchAsJsonAsync(
            "/api/settings/attendance-preference",
            new { Enabled = false },
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.Forbidden, updatePreferenceResponse.StatusCode);

        using HttpResponseMessage deleteAccountResponse = await SendDeleteAccountAsync(
            administratorClient
        );
        Assert.Equal(HttpStatusCode.Forbidden, deleteAccountResponse.StatusCode);
    }
}
