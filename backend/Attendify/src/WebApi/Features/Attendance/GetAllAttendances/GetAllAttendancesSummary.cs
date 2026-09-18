namespace Attendify.Features.Attendance.GetAllAttendances;

public class CreateAttendanceSummary : Summary<GetAllAttendancesEndpoint>
{
    public CreateAttendanceSummary()
    {
        Summary = "Get all attendance record.";
        Description = "Responds with all paginated attendance records. Returns a 200 OK.";

        ExampleRequest = new GetAllAttendancesRequest(
        );
    }
}