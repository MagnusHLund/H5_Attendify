using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Users;

namespace Attendify.Common.Domain.Attendance;

public sealed class AttendanceRecord : AggregateRoot<int>
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

    public TimeOnly ArrivalTime { get; set; }

    public TimeOnly DepartureTime { get; set; }

    public bool DepartureKnown { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public User User { get; set; } = null!;

    private AttendanceRecord() { }

    public static AttendanceRecord Create(
        int userId,
        string classroom,
        TimeOnly arrivalTime,
        TimeOnly departureTime,
        bool departureKnown,
        DateOnly attendanceDate
    )
    {
        return new AttendanceRecord
        {
            UserId = userId,
            Classroom = classroom,
            ArrivalTime = arrivalTime,
            DepartureTime = departureTime,
            DepartureKnown = departureKnown,
            AttendanceDate = attendanceDate,
        };
    }
}
