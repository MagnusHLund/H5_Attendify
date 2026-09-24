namespace Attendify.Features.Attendance;

public sealed class AttendanceGroup : Group
{
    public AttendanceGroup()
    {
        Configure("attendance", ep => ep.Description(x => x.ProducesProblemDetails(500)));
    }
}
