namespace Attendify.Features.Auth.Logout;

public sealed class LogoutSummary : Summary<LogoutEndpoint>
{
    public LogoutSummary()
    {
        Summary = "Logs out the current session";
        Description =
            "Revokes the refresh token cookie when present and clears the authentication cookies.";
        Response(204, "Logout succeeded. Authentication cookies were cleared.");
    }
}
