using Attendify.Common.Domain.Users;

namespace Attendify.IntegrationTests;

public sealed class PrivacyDomainTests
{
    [Fact]
    public void NewAccountEnablesFacialAttendance()
    {
        User user = User.Create(Guid.NewGuid(), " Student@Example.com ", "password-hash", "protected-id");

        Assert.True(user.AttendanceEnabled);
        Assert.False(user.IsDeleted);
        Assert.Null(user.DeletedAt);
    }

    [Fact]
    public void SoftDeletionReplacesIdentifiersAndDisablesAttendance()
    {
        User user = User.Create(Guid.NewGuid(), "student@example.com", "password-hash", "protected-id");
        user.AttendanceEnabled = true;
        DateTimeOffset deletedAt = DateTimeOffset.UtcNow;

        user.AnonymizeForDeletion("deleted@example.invalid", "random-protected-id", deletedAt);

        Assert.True(user.IsDeleted);
        Assert.Equal(deletedAt, user.DeletedAt);
        Assert.False(user.AttendanceEnabled);
        Assert.Equal("deleted@example.invalid", user.Email);
        Assert.Equal("random-protected-id", user.EncryptedStudentId);
    }
}
