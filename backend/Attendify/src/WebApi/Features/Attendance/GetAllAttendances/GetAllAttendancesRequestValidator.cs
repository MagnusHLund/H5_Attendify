using attendanceClass = Attendify.Common.Domain.Attendance.Attendance;
using Attendify.Common.Domain.Attendance;
using Attendify.Features.Attendance;
using Attendify.Common.Pagination;

namespace Attendify.Features.Attendance.GetAllAttendances;

public sealed class GetAllAttendancesRequestValidator
    : PagedRequestValidator<GetAllAttendancesRequest, attendanceClass>
{
    public GetAllAttendancesRequestValidator()
        : base(AttendanceSpec.SortColumns)
    {
    }
}