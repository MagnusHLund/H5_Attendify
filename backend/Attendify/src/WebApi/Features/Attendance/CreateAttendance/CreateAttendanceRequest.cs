namespace Attendify.Features.Attendance.CreateAttendance;

public sealed record CreateAttendanceRequest(IFormFile Picture, string Classroom);
