using attendanceClass = Attendify.Common.Domain.Attendance.Attendance;

using Attendify.Common.Domain.Attendance;
using Attendify.Common.Pagination;
using System.Security.Claims;
using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;

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

    public override async Task HandleAsync(
        GetAllAttendancesRequest req,
        CancellationToken ct)
    {
        PagingParams paging =
            PagingParams.From(req.Page, req.PageSize);

        var spec = AttendanceSpec.Paged(
            paging,
            req.SortBy,
            SortDirections.From(req.SortDirection));

        string? userIdClaim =
            User.FindFirstValue(AttendifyClaimTypes.UserId);

        if (userIdClaim is null ||
            !int.TryParse(userIdClaim, out int userIdValue))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        UserId userId = UserId.From(userIdValue);

        var attendances = await dbContext.Attendances
        .Where(a => a.UserId == userId)
        // Only return the first scan for each day
        .Where(a => a.Id == dbContext.Attendances
            .Where(x =>
                x.UserId == a.UserId &&
                x.AttendanceDate == a.AttendanceDate)
            .OrderBy(x => x.ArrivedAt)
            .Select(x => x.Id)
            .First())
        .ToPagedListAsync(
            spec,
            a => new GetAllAttendancesResponse(
                a.AttendanceDate,
                a.ArrivedAt,
            dbContext.Attendances
                .Where(x =>
                    x.UserId == a.UserId &&
                    x.AttendanceDate == a.AttendanceDate &&
                    x.Id != a.Id)
                .OrderByDescending(x => x.ArrivedAt)
                .Select(x => (TimeOnly?)x.ArrivedAt)
                .FirstOrDefault(),
            a.Classroom,
            a.Status.ToString()
        ), ct);

        await Send.OkAsync(attendances, ct);
    }
}