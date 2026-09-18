namespace Attendify.Features.Attendance.CreateAttendance;

public class CreateAttendanceSummary : Summary<CreateAttendanceEndpoint>
{
    public CreateAttendanceSummary()
    {
        Summary = "Create an attendance record";
        Description = "Creates an attendance record with a classroom and arrival time. Returns a 200 and no body.";

        ExampleRequest = new CreateAttendanceRequest(
                Classroom: "D16"
        );
    }
}