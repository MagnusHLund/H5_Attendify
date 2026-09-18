namespace Attendify.Features.Attendance.GetAllAttendances;

public class GetAllAttendancesEndpoint(ApplicationDbContext dbContext)
    : Endpoint<GetAllAttendancesRequest, GetAllAttendancesResponse>
{
    public override void Configure()
    {
        Get("/");
        Group<AttendanceGroup>();
        Description(x => x.WithName("GetAllAttendances"));
    }

    public override async Task HandleAsync(GetAllAttendancesRequest req, CancellationToken ct)
    {
    }
}