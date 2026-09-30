namespace Attendify.Features.Settings.GetAttendancePreference;

public sealed class GetAttendancePreferenceSummary : Summary<GetAttendancePreferenceEndpoint>
{
    public GetAttendancePreferenceSummary()
    {
        Summary = "Returns the authenticated user's attendance preference";
        Description = "Returns whether facial attendance recognition is enabled for the user.";
        Response<GetAttendancePreferenceResponse>(200, "The current attendance preference.");
        Response(401, "The request is not authenticated or the account has been deleted.");
    }
}
