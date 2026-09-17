namespace Attendify.Common.Domain.Attendance;

public static class AttendanceErrors
{
    public static readonly Error NotFound = Error.NotFound(
            "Attendance.NotFound",
            "Attendance is not found");
}