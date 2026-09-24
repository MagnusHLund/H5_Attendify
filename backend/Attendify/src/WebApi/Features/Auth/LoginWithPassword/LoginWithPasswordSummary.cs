namespace Attendify.Features.Auth.LoginWithPassword;

public sealed class LoginWithPasswordSummary : Summary<LoginWithPasswordEndpoint>
{
    public LoginWithPasswordSummary()
    {
        Summary = "Logs in with an email address and password";
        Description =
            "Validates the credentials, creates an authentication session, and sets the access and refresh token cookies.";
        Response(204, "Login succeeded. Authentication cookies were set.");
        Response(400, "The credentials are invalid or the request is invalid.");
    }
}
