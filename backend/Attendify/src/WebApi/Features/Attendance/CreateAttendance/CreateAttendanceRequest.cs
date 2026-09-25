namespace Attendify.Features.Attendance.CreateAttendance;

public sealed record CreateAttendanceRequest(
    string Picture,
    string Classroom
);