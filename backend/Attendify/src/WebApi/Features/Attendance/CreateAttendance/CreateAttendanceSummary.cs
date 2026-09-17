namespace Attendify.Features.Attendance.CreateAttendance;

public class CreateHeroSummary : Summary<CreateAttendanceEndpoint>
{
    public CreateHeroSummary()
    {
        Summary = "Create an attendance record";
        Description = "Creates an attendance record with a classroom and arrival time. Returns a 201 and no body.";

        ExampleRequest = new CreateAttendanceRequest(
                Classroom: "D16"
        );
    }
}