namespace Attendify.Features.Auth.RequestPasswordReset;

public sealed class RequestPasswordResetSummary : Summary<RequestPasswordResetEndpoint>
{
    public RequestPasswordResetSummary()
    {
        Summary = "Requests a password reset code";
        Description =
            "Sends a time-limited password reset code when the email belongs to an account. Returns the same response for known and unknown addresses.";
        Response(204, "The request was accepted.");
        Response(400, "The email address is invalid.");
    }
}
