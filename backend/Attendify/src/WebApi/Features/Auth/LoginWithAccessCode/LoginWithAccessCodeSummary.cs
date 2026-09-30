namespace Attendify.Features.Auth.LoginWithAccessCode;

public sealed class LoginWithAccessCodeSummary : Summary<LoginWithAccessCodeEndpoint>
{
    public LoginWithAccessCodeSummary()
    {
        Summary = "Logs in with a student access code";
        Description =
            "Validates the access code, creates an access-token-only session, and sets the access token cookie.";
        Response(204, "Login succeeded. The access token cookie was set.");
        Response(400, "The request is invalid.");
        Response(401, "The student access code is invalid, expired, or missing.");
    }
}