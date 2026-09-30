namespace Attendify.Features.Settings.DeleteAccount;

public sealed class DeleteAccountSummary : Summary<DeleteAccountEndpoint>
{
    public DeleteAccountSummary()
    {
        Summary = "Deletes the authenticated user's account";
        Description =
            "Verifies the current password, removes the user's facial profile, attendance data, access codes and tokens, anonymizes the retained account row, and clears the authentication cookies.";
        Response(204, "The account was deleted and the session was ended.");
        Response(400, "The current password is missing or incorrect.");
        Response(401, "The request is not authenticated or the account has already been deleted.");
    }
}
