namespace Attendify.Features.Settings.UpdateAttendancePreference;

public sealed class UpdateAttendancePreferenceSummary : Summary<UpdateAttendancePreferenceEndpoint>
{
    public UpdateAttendancePreferenceSummary()
    {
        Summary = "Enables or disables facial attendance recognition";
        Description =
            "Disabling recognition permanently removes the user's facial profile and embeddings. Enabling it requires face photos to have been added first.";
        Response(204, "The attendance preference was updated.");
        Response(400, "The request is invalid.");
        Response(401, "The request is not authenticated or the account has been deleted.");
        Response(403, "Only student sessions can use this endpoint.");
        Response(409, "Recognition cannot be enabled because no face photos have been added.");
    }
}
