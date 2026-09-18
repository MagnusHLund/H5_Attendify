namespace Attendify.Features.Attendance.GetAllAttendances;

public class GetAllAttendancesSummary : Summary<GetAllAttendancesEndpoint>
{
    public GetAllAttendancesSummary()
    {
        Summary = "Get all attendance records.";
        Description = "Responds with all paginated attendance records. Returns a 200 OK.";

        ExampleRequest = new GetAllAttendancesRequest(
        );
    }
}