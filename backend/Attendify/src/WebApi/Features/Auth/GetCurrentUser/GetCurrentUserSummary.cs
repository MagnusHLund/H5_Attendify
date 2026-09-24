namespace Attendify.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserSummary : Summary<GetCurrentUserEndpoint>
{
    public GetCurrentUserSummary()
    {
        Summary = "Gets the authenticated user's details";
        Description =
            "Returns the user type and student ID from the authenticated user's claims.";
        Response<GetCurrentUserResponse>(200, "The authenticated user's details.");
        Response(401, "The request is not authenticated.");
        Response(403, "The authentication token does not contain valid user claims.");
    }
}
