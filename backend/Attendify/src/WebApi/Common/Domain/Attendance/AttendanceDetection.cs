using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Users;

namespace Attendify.Common.Domain.Attendance;

public sealed class AttendanceDetection : AggregateRoot<int>
{
    public const int ClassroomMaxLength = 100;

    public int UserId { get; set; }

    public string Classroom
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Classroom));
            ThrowIfGreaterThan(value.Length, ClassroomMaxLength, nameof(Classroom));
            field = value;
        }
    } = null!;

    public DateTimeOffset DetectedAt { get; set; }

    public User User { get; set; } = null!;

    private AttendanceDetection() { }

    public static AttendanceDetection Create(
        int userId,
        string classroom,
        DateTimeOffset detectedAt
    )
    {
        return new AttendanceDetection
        {
            UserId = userId,
            Classroom = classroom,
            DetectedAt = detectedAt,
        };
    }
}
