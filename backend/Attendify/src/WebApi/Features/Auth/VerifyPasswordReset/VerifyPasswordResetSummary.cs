namespace Attendify.Features.Auth.VerifyPasswordReset;

public sealed class VerifyPasswordResetSummary : Summary<VerifyPasswordResetEndpoint>
{
    public VerifyPasswordResetSummary()
    {
        Summary = "Verifies a password reset code";
        Description =
            "Checks that the supplied code matches the reset request for the email and has not expired.";
        Response(204, "The code is valid.");
        Response(400, "The code is invalid, expired, or the request data is invalid.");
    }
}
