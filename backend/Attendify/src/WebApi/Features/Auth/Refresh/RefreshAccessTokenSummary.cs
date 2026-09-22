namespace Attendify.Features.Auth.Refresh;

public class RefreshAccessTokenSummary : Summary<RefreshAccessTokenEndpoint>
{
    public RefreshAccessTokenSummary()
    {
        Summary = "Refreshes an access token";
        Description = "Generates a new access token using a valid refresh token.";
        Response(204);
    }
}
