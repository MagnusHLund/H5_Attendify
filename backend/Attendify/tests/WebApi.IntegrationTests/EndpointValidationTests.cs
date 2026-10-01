using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Attendify.Features.Settings.UpdateAttendancePreference;
using Attendify.Features.Settings.UpdateFacePictures;
using Attendify.IntegrationTests.Common;

namespace Attendify.IntegrationTests;

public sealed class EndpointValidationTests(TestingDatabaseFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Theory]
    [InlineData("/api/auth/login", """{"Email":"","Password":"password123"}""")]
    [InlineData("/api/auth/login", """{"Email":"not-an-email","Password":"password123"}""")]
    [InlineData("/api/auth/login", """{"Email":"user@example.com","Password":"short"}""")]
    [InlineData("/api/auth/login", """{"Email":"user@example.com","Password":""}""")]
    [InlineData("/api/auth/register", """{}""")]
    [InlineData("/api/auth/register", """{"Email":"bad","Password":"password123","EducationalInstituteId":"11111111-1111-1111-1111-111111111111","StudentId":"1","StraightPhoto":"AQID","LeftPhoto":"AQID","RightPhoto":"AQID"}""")]
    [InlineData("/api/auth/register", """{"Email":"user@example.com","Password":"short","EducationalInstituteId":"11111111-1111-1111-1111-111111111111","StudentId":"1","StraightPhoto":"AQID","LeftPhoto":"AQID","RightPhoto":"AQID"}""")]
    [InlineData("/api/auth/login/access-code", """{"StudentAccessCode":""}""")]
    [InlineData("/api/auth/password-reset/request", """{"Email":"invalid"}""")]
    [InlineData("/api/auth/password-reset/verify", """{"Email":"user@example.com","SecurityCode":"bad"}""")]
    [InlineData("/api/auth/password-reset/complete", """{"Email":"user@example.com","SecurityCode":"bad","NewPassword":"password123"}""")]
    [InlineData("/api/auth/password-reset/complete", """{"Email":"user@example.com","SecurityCode":"A23456789","NewPassword":"short"}""")]
    public async Task JsonEndpoints_RejectInvalidRequestBodies(string path, string json)
    {
        using HttpClient client = GetAnonymousClient();
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await client.PostAsync(path, content, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RequestValidators_RejectValuesBeyondConfiguredMaximumLengths()
    {
        using HttpClient client = GetAnonymousClient();
        string longEmail = $"{new string('a', 309)}@example.com";
        string validInstituteId = Guid.NewGuid().ToString();
        (string Path, string Json)[] requests =
        [
            (
                "/api/auth/login",
                JsonSerializer.Serialize(new { Email = longEmail, Password = "password123" })
            ),
            (
                "/api/auth/login",
                JsonSerializer.Serialize(new
                {
                    Email = "user@example.com",
                    Password = new string('p', 129)
                })
            ),
            (
                "/api/auth/register",
                JsonSerializer.Serialize(new
                {
                    Email = "user@example.com",
                    Password = "password123",
                    EducationalInstituteId = validInstituteId,
                    StudentId = new string('s', 129),
                    StraightPhoto = "AQID",
                    LeftPhoto = "AQID",
                    RightPhoto = "AQID"
                })
            ),
            (
                "/api/auth/password-reset/complete",
                JsonSerializer.Serialize(new
                {
                    Email = "user@example.com",
                    SecurityCode = "A23456789",
                    NewPassword = new string('p', 129)
                })
            )
        ];

        foreach ((string path, string json) in requests)
        {
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await client.PostAsync(path, content, CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    [Fact]
    public async Task UpdateAttendancePreference_RejectsNullEnabledValue()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage response = await client.PatchAsJsonAsync(
            "/api/settings/attendance-preference",
            new UpdateAttendancePreferenceRequest(null),
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFacePictures_RejectsInvalidBase64()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage response = await client.PatchAsJsonAsync(
            "/api/settings/face-photos",
            new UpdateFacePicturesRequest
            {
                StraightPhoto = "not base64!",
                LeftPhoto = "AQID",
                RightPhoto = "AQID"
            },
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("", "AQID", "AQID")]
    [InlineData("AQID", "", "AQID")]
    [InlineData("AQID", "AQID", "")]
    public async Task UpdateFacePictures_RequiresAllThreePhotos(
        string straightPhoto,
        string leftPhoto,
        string rightPhoto
    )
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage response = await client.PatchAsJsonAsync(
            "/api/settings/face-photos",
            new UpdateFacePicturesRequest
            {
                StraightPhoto = straightPhoto,
                LeftPhoto = leftPhoto,
                RightPhoto = rightPhoto
            },
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFacePictures_RejectsPhotosAboveMaximumEncodedLength()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage response = await client.PatchAsJsonAsync(
            "/api/settings/face-photos",
            new UpdateFacePicturesRequest
            {
                StraightPhoto = new string('A', 6_990_512),
                LeftPhoto = "AQID",
                RightPhoto = "AQID"
            },
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_RejectsEmptyPassword()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();
        using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/settings/account")
        {
            Content = JsonContent.Create(new { CurrentPassword = "" })
        };

        using HttpResponseMessage response = await client.SendAsync(request, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("?sortBy=UnknownColumn")]
    [InlineData("?sortDirection=sideways")]
    public async Task GetAttendances_RejectsUnsupportedSortParameters(string query)
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();

        using HttpResponseMessage response = await client.GetAsync(
            $"/api/attendance{query}",
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAttendance_RejectsNonJpegUploads()
    {
        using HttpClient client = GetAnonymousClient();
        using var content = new MultipartFormDataContent();
        using var image = new ByteArrayContent([1, 2, 3]);
        image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(image, "Picture", "photo.png");
        content.Add(new StringContent("Room 1"), "Classroom");

        using HttpResponseMessage response = await client.PostAsync(
            "/api/attendance",
            content,
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("image/png", "Room 1", 3)]
    [InlineData("image/jpeg", "", 3)]
    [InlineData("image/jpeg", "Room 1", 0)]
    [InlineData("image/jpeg", "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", 3)]
    public async Task CreateAttendance_RejectsInvalidMultipartFields(
        string contentType,
        string classroom,
        int imageLength
    )
    {
        using HttpClient client = GetAnonymousClient();
        using var content = new MultipartFormDataContent();
        using var image = new ByteArrayContent(new byte[imageLength]);
        image.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(image, "Picture", "photo.jpg");
        content.Add(new StringContent(classroom), "Classroom");

        using HttpResponseMessage response = await client.PostAsync(
            "/api/attendance",
            content,
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAttendance_RejectsPhotosLargerThanFiveMegabytes()
    {
        using HttpClient client = GetAnonymousClient();
        using var content = new MultipartFormDataContent();
        using var image = new ByteArrayContent(new byte[5 * 1024 * 1024 + 1]);
        image.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(image, "Picture", "photo.jpg");
        content.Add(new StringContent("Room 1"), "Classroom");

        using HttpResponseMessage response = await client.PostAsync(
            "/api/attendance",
            content,
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAttendance_RequiresAnUploadedPhoto()
    {
        using HttpClient client = GetAnonymousClient();
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Room 1"), "Classroom");

        using HttpResponseMessage response = await client.PostAsync(
            "/api/attendance",
            content,
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
