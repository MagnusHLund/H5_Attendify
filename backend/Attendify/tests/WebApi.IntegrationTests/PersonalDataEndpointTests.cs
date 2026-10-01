using System.Net;
using System.Net.Http.Json;
using Attendify.Common.Domain.Users;
using Attendify.Common.Pagination;
using Attendify.Common.Persistence;
using Attendify.Features.Attendance.GetAllAttendances;
using Attendify.Features.Settings.DeleteAccount;
using Attendify.Features.Settings.ExportPersonalData;
using Attendify.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using attendanceClass = Attendify.Common.Domain.Attendance.Attendance;

namespace Attendify.IntegrationTests;

public sealed class PersonalDataEndpointTests(TestingDatabaseFixture fixture)
    : IntegrationTestBase(fixture)
{
    private const string Password = "correct horse battery";

    [Fact]
    public async Task ExportPersonalData_ReturnsAccountAndAttendanceDataWithoutCaching()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();
        User user = await GetService<ApplicationDbContext>()
            .Users.AsNoTracking()
            .SingleAsync(cancellationToken: CancellationToken);
        attendanceClass attendance = attendanceClass.Create("Room 12", UserId.From(user.Id));
        attendance.SetCreated(TimeProvider.System, null);
        await AddAsync(attendance);
        await AddOtherUsersAttendanceAsync(user);

        using HttpResponseMessage response = await client.GetAsync(
            "/api/settings/personal-data",
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Contains(
            "attachment; filename=attendify-personal-data.json",
            response.Content.Headers.ContentDisposition?.ToString()
        );
        ExportPersonalDataResponse? export = await response.Content
            .ReadFromJsonAsync<ExportPersonalDataResponse>(CancellationToken);
        Assert.NotNull(export);
        Assert.Equal("student@example.com", export.Account.Email);
        Assert.Equal("student-123", export.Account.StudentId);
        ExportPersonalDataResponse.AttendanceEventData eventData = Assert.Single(
            export.AttendanceEvents
        );
        Assert.Equal("Room 12", eventData.Classroom);
        Assert.Equal("Present", eventData.Status);
    }

    [Fact]
    public async Task GetAttendances_ClampsPagingAndReturnsTheCurrentUsersEvents()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();
        User user = await GetService<ApplicationDbContext>()
            .Users.AsNoTracking()
            .SingleAsync(cancellationToken: CancellationToken);
        attendanceClass attendance = attendanceClass.Create("Room 12", UserId.From(user.Id));
        attendance.SetCreated(TimeProvider.System, null);
        await AddAsync(attendance);
        await AddOtherUsersAttendanceAsync(user);

        using HttpResponseMessage response = await client.GetAsync(
            "/api/attendance?page=0&pageSize=500&sortBy=classroom&sortDirection=descending",
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PagedList<GetAllAttendancesResponse>? page = await response.Content
            .ReadFromJsonAsync<PagedList<GetAllAttendancesResponse>>(CancellationToken);
        Assert.NotNull(page);
        Assert.Equal(1, page.Page);
        Assert.Equal(100, page.PageSize);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal("Room 12", Assert.Single(page.Items).Classroom);
    }

    [Fact]
    public async Task DeleteAccount_RejectsWrongPasswordThenAnonymizesAccount()
    {
        using HttpClient client = await CreateAuthenticatedStudentClientAsync();
        using HttpResponseMessage wrongPasswordResponse = await SendDeleteAccountAsync(
            client,
            "wrong password"
        );
        Assert.Equal(HttpStatusCode.BadRequest, wrongPasswordResponse.StatusCode);

        using HttpResponseMessage deleteResponse = await SendDeleteAccountAsync(client, Password);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        User deletedUser = await GetService<ApplicationDbContext>()
            .Users.AsNoTracking()
            .SingleAsync(cancellationToken: CancellationToken);
        Assert.True(deletedUser.IsDeleted);
        Assert.False(deletedUser.AttendanceEnabled);
        Assert.NotNull(deletedUser.DeletedAt);
        Assert.EndsWith("@deleted.invalid", deletedUser.Email);

        using HttpResponseMessage currentUserResponse = await client.GetAsync(
            "/api/auth/me",
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.Unauthorized, currentUserResponse.StatusCode);
    }

    private async Task<HttpResponseMessage> SendDeleteAccountAsync(HttpClient client, string password)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, "/api/settings/account")
        {
            Content = JsonContent.Create(new DeleteAccountRequest(password))
        };
        return await client.SendAsync(request, CancellationToken);
    }

    private async Task AddOtherUsersAttendanceAsync(User existingUser)
    {
        User otherUser = User.Create(
            existingUser.EducationalInstituteId,
            "other-student@example.com",
            "not-used-by-this-test",
            "protected-other-student-id"
        );
        otherUser.SetCreated(TimeProvider.System, null);
        await AddAsync(otherUser);

        attendanceClass otherAttendance = attendanceClass.Create(
            "Other Room",
            UserId.From(otherUser.Id)
        );
        otherAttendance.SetCreated(TimeProvider.System, null);
        await AddAsync(otherAttendance);
    }
}
