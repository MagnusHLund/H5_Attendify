namespace Attendify.Features.Attendance.CreateAttendance;

public class CreateAttendanceSummary : Summary<CreateAttendanceEndpoint>
{
    public CreateAttendanceSummary()
    {
        Summary = "Create an attendance record";
        Description = "Creates an attendance record with a classroom, picture and arrival time. Returns a 201 and no body.";

        ExampleRequest = new CreateAttendanceRequest(
                Classroom: "D16",
                Picture: Convert.ToBase64String(new byte[100])
        );
    }
}