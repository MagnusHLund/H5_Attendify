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
        int userId = Random.Shared.Next(); //INFO: Temporary until we can get JWT claim's userId

        var attendance = attendanceClass.Attendance.Create(req.Classroom, userId);

        dbContext.Attendances.Add(attendance);
        await dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(cancellation: ct);
    }
}