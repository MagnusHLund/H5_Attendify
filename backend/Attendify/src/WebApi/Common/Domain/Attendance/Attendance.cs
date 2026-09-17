using Attendify.Common.Domain.Base;

namespace Attendify.Common.Domain.Attendance;

[ValueObject<int>]
public readonly partial struct AttendanceId;

public sealed class Attendance : AggregateRoot<AttendanceId>
{
    public const int ClassroomMaxLength = 100;

    // TODO: Change to "UserId" once the UserId has been implemented, then make new migrations.
    public int UserId
    {
        get;
        set
        {
            field = value;
        }
    }

    public DateOnly AttendanceDate
    {
        get;
        set
        {
            field = value;
        }
    }

    public TimeOnly ArrivedAt
    {
        get;
        set
        {
            field = value;
        }
    }

    public TimeOnly? DepartedAt
    {
        get;
        set
        {
            field = value;

        }
    }

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

    public AttendanceStatus Status
    {
        get;
        set
        {
            field = value;
        }
    }

    private Attendance() { }

    public static Attendance Create(
            string classroom
    )
    {
        DateTime currentDatetime = DateTime.Now;

        Attendance attendance = new Attendance
        {
            AttendanceDate = DateOnly.FromDateTime(currentDatetime),
            ArrivedAt = TimeOnly.FromDateTime(currentDatetime),
            Classroom = classroom,
            Status = AttendanceStatus.Present
        };

        return attendance;
    }
}