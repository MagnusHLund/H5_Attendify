namespace Attendify.Features.Auth;

public sealed class AuthenticationGroup : Group
{
    public AuthenticationGroup()
    {
        Configure(
            "auth",
            ep =>
            {
                ep.Description(x => x.WithTags("Authentication"));
            }
        );
    }
}
