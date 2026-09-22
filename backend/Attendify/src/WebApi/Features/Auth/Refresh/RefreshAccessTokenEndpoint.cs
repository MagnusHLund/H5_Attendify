using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;

namespace Attendify.Features.Auth.Refresh;

public sealed class RefreshAccessTokenEndpoint(
    IRefreshTokenService refreshTokenService,
    IAuthenticationSessionService authenticationSessionService
) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/refresh");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("RefreshToken"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? refreshToken = HttpContext.Request.Cookies[
            AuthenticationCookieService.RefreshTokenCookieName
        ];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        bool refreshed = await authenticationSessionService.RefreshSessionAsync(refreshToken, ct);

        if (!refreshed)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
