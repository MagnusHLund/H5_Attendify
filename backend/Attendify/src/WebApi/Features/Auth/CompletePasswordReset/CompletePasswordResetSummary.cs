namespace Attendify.Features.Auth.CompletePasswordReset;

public sealed class CompletePasswordResetSummary : Summary<CompletePasswordResetEndpoint>
{
    public CompletePasswordResetSummary()
    {
        Summary = "Completes a password reset";
        Description =
            "Validates the reset code, replaces the user's password hash, and consumes the reset code.";
        Response(204, "The password was changed successfully.");
        Response(400, "The code is invalid, expired, or the request data is invalid.");
    }
}
