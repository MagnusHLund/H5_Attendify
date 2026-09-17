namespace Attendify.Features.Attendance;

public class AttendanceGroup : Group
{
    public AttendanceGroup()
    {
        base.Configure("attendance", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}