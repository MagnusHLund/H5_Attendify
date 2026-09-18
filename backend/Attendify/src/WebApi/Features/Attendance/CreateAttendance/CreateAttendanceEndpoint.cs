namespace Attendify.Features.Attendance.CreateAttendance;

using attendanceClass = Common.Domain.Attendance;

public class CreateAttendanceEndpoint(ApplicationDbContext dbContext) : Endpoint<CreateAttendanceRequest> //TODO: inherit from endpoint with request response  
{
    public override void Configure()
    {
        Post("/");
        Group<AttendanceGroup>();
        Description(x => x.WithName("CreateAttendance"));
    }

    public override async Task HandleAsync(CreateAttendanceRequest req, CancellationToken ct)
    {
    }
}