using attendanceClass = Attendify.Common.Domain.Attendance.Attendance;

using Attendify.Common.Domain.Attendance;
using Attendify.Common.Pagination;

namespace Attendify.Features.Attendance.GetAllAttendances;

public class GetAllAttendancesEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetAllAttendancesRequest, PagedList<GetAllAttendancesResponse>>
{
    public override void Configure()
    {
        Get("/");
        Group<AttendanceGroup>();
        Description(x => x.WithName("GetAllAttendances"));
    }

    public override async Task HandleAsync(GetAllAttendancesRequest req, CancellationToken ct)
    {
        PagingParams paging = PagingParams.From(req.Page, req.PageSize);
        var spec = AttendanceSpec.Paged(paging, req.SortBy, SortDirections.From(req.SortDirection));

        var attendances = await dbContext.Attendances.ToPagedListAsync(
                spec,
                a => new GetAllAttendancesResponse(
                    a.AttendanceDate,
                    a.ArrivedAt,
                    a.DepartedAt,
                    a.Classroom,
                    a.Status.ToString()
                ), ct);

        await Send.OkAsync(attendances, ct);
    }
}