using Attendify.Common.Pagination;

namespace Attendify.Features.Attendance.GetAllAttendances;

public sealed record GetAllAttendancesResponse(
    DateOnly AttendanceDate,
    TimeOnly ArrivedAt,
    TimeOnly? DepartedAt,
    string Classroom,
    string Status
);