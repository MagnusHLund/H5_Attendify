using Attendify.Common.Authentication;

namespace Attendify.Features.Auth.Logout;

public sealed class LogoutEndpoint(
    IRefreshTokenService refreshTokenService,
    IAuthenticationCookieService authenticationCookieService
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/logout");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("Logout"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? refreshToken = HttpContext.Request.Cookies[
            AuthenticationCookieService.RefreshTokenCookieName
        ];

        try
        {
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await refreshTokenService.RevokeTokenAsync(refreshToken, ct);
            }
        }
        finally
        {
            authenticationCookieService.ClearAuthenticationCookies();
        }

        await Send.NoContentAsync(ct);
    }
}
