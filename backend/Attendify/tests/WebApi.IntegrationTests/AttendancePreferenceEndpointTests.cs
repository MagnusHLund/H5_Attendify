using System.Net;
using System.Net.Http.Json;
using Attendify.Common.Domain.FacialRecognition;
using Attendify.Common.Domain.Users;
using Attendify.Common.Persistence;
using Attendify.Features.Settings.GetAttendancePreference;
using Attendify.Features.Settings.UpdateAttendancePreference;
using Attendify.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace Attendify.IntegrationTests;

public sealed class AttendancePreferenceEndpointTests(TestingDatabaseFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task AttendancePreference_CanBeReadAndDisabled()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage initialResponse = await client.GetAsync(
            "/api/settings/attendance-preference",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.OK, initialResponse.StatusCode);
        GetAttendancePreferenceResponse? initialPreference = await initialResponse.Content
            .ReadFromJsonAsync<GetAttendancePreferenceResponse>(CancellationToken);
        Assert.NotNull(initialPreference);
        Assert.True(initialPreference.Enabled);

        using HttpResponseMessage updateResponse = await client.PatchAsJsonAsync(
            "/api/settings/attendance-preference",
            new UpdateAttendancePreferenceRequest(false),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        using HttpResponseMessage updatedResponse = await client.GetAsync(
            "/api/settings/attendance-preference",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.OK, updatedResponse.StatusCode);
        GetAttendancePreferenceResponse? updatedPreference = await updatedResponse.Content
            .ReadFromJsonAsync<GetAttendancePreferenceResponse>(CancellationToken);
        Assert.NotNull(updatedPreference);
        Assert.False(updatedPreference.Enabled);
    }

    [Fact]
    public async Task AttendancePreference_CannotBeEnabledWithoutFaceProfile()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage response = await client.PatchAsJsonAsync(
            "/api/settings/attendance-preference",
            new UpdateAttendancePreferenceRequest(true),
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AttendancePreference_CanBeEnabledWithStoredFaceEmbedding()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();
        User user = await GetService<ApplicationDbContext>()
            .Users.AsNoTracking()
            .SingleAsync(cancellationToken: CancellationToken);
        FacialProfile profile = FacialProfile.Create(user.Id);
        profile.SetCreated(TimeProvider.System, null);
        profile.FacialEmbeddings.Add(FacialEmbedding.Create([1, 2, 3], [4, 5, 6]));
        await AddAsync(profile);

        using HttpResponseMessage updateResponse = await client.PatchAsJsonAsync(
            "/api/settings/attendance-preference",
            new UpdateAttendancePreferenceRequest(true),
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        using HttpResponseMessage getResponse = await client.GetAsync(
            "/api/settings/attendance-preference",
            CancellationToken
        );
        GetAttendancePreferenceResponse? preference = await getResponse.Content
            .ReadFromJsonAsync<GetAttendancePreferenceResponse>(CancellationToken);
        Assert.NotNull(preference);
        Assert.True(preference.Enabled);
    }

    [Fact]
    public async Task AttendancePreference_RequiresStudentAuthentication()
    {
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage response = await client.GetAsync(
            "/api/settings/attendance-preference",
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
