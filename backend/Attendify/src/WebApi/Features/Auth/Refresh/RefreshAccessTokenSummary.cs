namespace Attendify.Features.Auth.Refresh;

public class RefreshAccessTokenSummary : Summary<RefreshAccessTokenEndpoint>
{
    public RefreshAccessTokenSummary()
    {
        Summary = "Refreshes the authentication session";
        Description =
            "Rotates a valid refresh token and sets new access and refresh token cookies.";
        Response(204, "The session was refreshed and authentication cookies were set.");
        Response(401, "The refresh token is missing, expired, revoked, or invalid.");
    }
}
