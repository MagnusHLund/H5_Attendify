using Attendify.Common.Domain.EducationalInstitute;
using Attendify.Common.Domain.Attendance;
using Attendify.Common.Domain.Users;
using Attendify.Common.Persistence;
using Attendify.Features.Settings.ExportPersonalData;
using Attendify.IntegrationTests.Common.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Attendify.IntegrationTests;

public sealed class ActiveUserLockTests : IAsyncLifetime
{
    private readonly TestDatabase _database = new();

    public async ValueTask InitializeAsync() => await _database.InitializeAsync();

    public async ValueTask DisposeAsync() => await _database.DisposeAsync();

    [Fact]
    public async Task AcquireAsync_OnlyLocksActiveUsers()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_database.DbConnection.ConnectionString)
            .Options;
        await using var db = new ApplicationDbContext(options);
        EducationalInstitute institute = EducationalInstitute.Create("Test school");
        institute.SetCreated(TimeProvider.System, null);
        db.EducationalInstitutes.Add(institute);
        await db.SaveChangesAsync();
        User user = User.Create(institute.Id, "student@example.com", "hash", "protected-id");
        user.SetCreated(TimeProvider.System, null);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        await using var transaction = await db.Database.BeginTransactionAsync();

        Assert.True(await ActiveUserLock.AcquireAsync(db, user.Id, TestContext.Current.CancellationToken));
        Assert.False(await ActiveUserLock.AcquireAsync(db, -1, TestContext.Current.CancellationToken));
        await transaction.CommitAsync();

        user.AnonymizeForDeletion("deleted@deleted.invalid", "random-id", DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        await using var deletedTransaction = await db.Database.BeginTransactionAsync();
        Assert.False(await ActiveUserLock.AcquireAsync(db, user.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task AttendanceUserIdFiltersWorkForExportAndDeletion()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_database.DbConnection.ConnectionString)
            .Options;
        await using var db = new ApplicationDbContext(options);
        EducationalInstitute institute = EducationalInstitute.Create("Test school");
        institute.SetCreated(TimeProvider.System, null);
        db.EducationalInstitutes.Add(institute);
        User student = User.Create(institute.Id, "one@example.com", "hash", "protected-id");
        student.SetCreated(TimeProvider.System, null);
        User other = User.Create(institute.Id, "two@example.com", "hash", "protected-id");
        other.SetCreated(TimeProvider.System, null);
        db.Users.AddRange(student, other);
        await db.SaveChangesAsync();

        Attendance first = Attendance.Create("Class A", UserId.From(student.Id));
        first.SetCreated(TimeProvider.System, null);
        Attendance second = Attendance.Create("Class B", UserId.From(other.Id));
        second.SetCreated(TimeProvider.System, null);
        db.Attendances.AddRange(first, second);
        await db.SaveChangesAsync();

        UserId studentId = UserId.From(student.Id);
        ExportPersonalDataResponse.AttendanceEventData[] events = await db.Attendances.AsNoTracking()
            .Where(attendance => attendance.UserId == studentId)
            .OrderBy(attendance => attendance.AttendanceDate)
            .Select(attendance => new ExportPersonalDataResponse.AttendanceEventData(
                attendance.Classroom,
                attendance.AttendanceDate,
                attendance.ArrivedAt,
                attendance.Status.ToString()
            ))
            .ToArrayAsync();
        Assert.Single(events);
        Assert.Equal("Class A", events[0].Classroom);
        Assert.Equal("Present", events[0].Status);

        int deleted = await db.Attendances
            .Where(attendance => attendance.UserId == studentId)
            .ExecuteDeleteAsync();
        Assert.Equal(1, deleted);
        Assert.Equal(1, await db.Attendances.CountAsync());
    }
}
