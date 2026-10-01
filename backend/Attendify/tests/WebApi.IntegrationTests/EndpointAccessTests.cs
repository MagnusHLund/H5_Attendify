using System.Net;
using System.Net.Http.Json;
using Attendify.IntegrationTests.Common;

namespace Attendify.IntegrationTests;

public sealed class EndpointAccessTests(TestingDatabaseFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Theory]
    [InlineData("GET", "/api/attendance")]
    [InlineData("GET", "/api/settings/student-access-code")]
    [InlineData("GET", "/api/settings/personal-data")]
    [InlineData("PATCH", "/api/settings/face-photos")]
    [InlineData("DELETE", "/api/settings/account")]
    public async Task ProtectedEndpoints_RejectAnonymousRequests(string method, string path)
    {
        using HttpClient client = GetAnonymousClient();
        using HttpRequestMessage request = new(new HttpMethod(method), path);
        if (!HttpMethod.Get.Method.Equals(method, StringComparison.OrdinalIgnoreCase))
            request.Content = JsonContent.Create(new { });

        using HttpResponseMessage response = await client.SendAsync(request, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/login/access-code")]
    [InlineData("/api/auth/password-reset/request")]
    [InlineData("/api/auth/password-reset/verify")]
    [InlineData("/api/auth/password-reset/complete")]
    public async Task PublicEndpoints_ValidateRequiredInput(string path)
    {
        using HttpClient client = GetAnonymousClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(path, new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAttendance_RejectsJsonInsteadOfMultipartFormData()
    {
        using HttpClient client = GetAnonymousClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/attendance",
            new { },
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }
}
