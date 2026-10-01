using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Attendify.Common.Domain.EducationalInstitute;
using Attendify.Common.Domain.Users;
using Attendify.Common.Persistence;
using Attendify.Features.Auth.RegisterStudent;
using Attendify.Features.Settings.UpdateAttendancePreference;
using Attendify.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace Attendify.IntegrationTests;

public sealed class RegistrationAndAttendanceEndpointTests(TestingDatabaseFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task RegisterStudent_CreatesAccountProfileAndAuthenticatedSession()
    {
        EducationalInstitute institute = EducationalInstitute.Create("Registration School");
        institute.SetCreated(TimeProvider.System, null);
        await AddAsync(institute);
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/register",
            CreateRegistrationRequest(institute.Id, "new-student@example.com"),
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using HttpResponseMessage currentUserResponse = await client.GetAsync(
            "/api/auth/me",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.OK, currentUserResponse.StatusCode);
        using JsonDocument currentUser = await JsonDocument.ParseAsync(
            await currentUserResponse.Content.ReadAsStreamAsync(CancellationToken),
            cancellationToken: CancellationToken
        );
        Assert.Equal(
            "new-student-123",
            currentUser.RootElement.GetProperty("studentId").GetString()
        );

        User registeredUser = await GetService<ApplicationDbContext>()
            .Users.AsNoTracking()
            .Include(user => user.FacialProfile)
            .ThenInclude(profile => profile!.FacialEmbeddings)
            .SingleAsync(cancellationToken: CancellationToken);
        Assert.Equal("new-student@example.com", registeredUser.Email);
        Assert.Equal(3, registeredUser.FacialProfile!.FacialEmbeddings.Count);
    }

    [Fact]
    public async Task RegisterStudent_RejectsDuplicateEmailAndUnknownInstitute()
    {
        using HttpClient seedClient = await CreateAuthenticatedStudentClientAsync();
        using HttpClient client = GetAnonymousClient();

        using HttpResponseMessage duplicateEmailResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            CreateRegistrationRequest(Guid.NewGuid(), "student@example.com"),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.Conflict, duplicateEmailResponse.StatusCode);

        using HttpResponseMessage unknownInstituteResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            CreateRegistrationRequest(Guid.NewGuid(), "new-student@example.com"),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.BadRequest, unknownInstituteResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateFacePictures_StoresNewProfileAndAllowsAttendancePreference()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage updatePhotosResponse = await client.PatchAsJsonAsync(
            "/api/settings/face-photos",
            new
            {
                StraightPhoto = "AQID",
                LeftPhoto = "BAUG",
                RightPhoto = "BwgJ"
            },
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, updatePhotosResponse.StatusCode);

        using HttpResponseMessage preferenceResponse = await client.PatchAsJsonAsync(
            "/api/settings/attendance-preference",
            new UpdateAttendancePreferenceRequest(true),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, preferenceResponse.StatusCode);

        using HttpResponseMessage attendanceResponse = await client.PostAsync(
            "/api/attendance",
            CreateAttendanceRequest("Room 3"),
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.Created, attendanceResponse.StatusCode);

        using HttpResponseMessage attendanceListResponse = await client.GetAsync(
            "/api/attendance",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.OK, attendanceListResponse.StatusCode);
    }

    [Fact]
    public async Task CreateAttendance_ReturnsNotFoundWhenNoFaceProfileCanMatch()
    {
        using HttpClient client = GetAnonymousClient();
        using HttpResponseMessage response = await client.PostAsync(
            "/api/attendance",
            CreateAttendanceRequest("Room 3"),
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static RegisterStudentRequest CreateRegistrationRequest(Guid instituteId, string email) =>
        new()
        {
            Email = email,
            Password = "new student password",
            EducationalInstituteId = instituteId,
            StudentId = "new-student-123",
            StraightPhoto = "AQID",
            LeftPhoto = "BAUG",
            RightPhoto = "BwgJ"
        };

    private static MultipartFormDataContent CreateAttendanceRequest(string classroom)
    {
        var content = new MultipartFormDataContent();
        var image = new ByteArrayContent([1, 2, 3]);
        image.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(image, "Picture", "photo.jpg");
        content.Add(new StringContent(classroom), "Classroom");
        return content;
    }
}
