using Microsoft.Extensions.Options;

namespace Attendify.Common.Authentication;

public class AuthenticationCookieService : IAuthenticationCookieService
{
    public const string AccessTokenCookieName = "AccessToken";
    public const string RefreshTokenCookieName = "RefreshToken";

    private readonly JwtOptions _jwtOptions;
    private readonly RefreshTokenOptions _refreshTokenOptions;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationCookieService(
        IHttpContextAccessor httpContextAccessor,
        IOptions<JwtOptions> jwtOptions,
        IOptions<RefreshTokenOptions> refreshTokenOptions
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtOptions = jwtOptions.Value;
        _refreshTokenOptions = refreshTokenOptions.Value;
    }

    public void SetAccessTokenCookie(string accessToken)
    {
        _httpContextAccessor?.HttpContext?.Response?.Cookies.Append(
            AccessTokenCookieName,
            accessToken,
            CreateCookieOptions(_jwtOptions.AccessTokenLifetimeMinutes)
        );
    }

    public void SetRefreshTokenCookie(string refreshToken)
    {
        _httpContextAccessor?.HttpContext?.Response?.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            CreateCookieOptions(_refreshTokenOptions.LifetimeMinutes)
        );
    }

    public void ClearAuthenticationCookies()
    {
        _httpContextAccessor?.HttpContext?.Response?.Cookies.Delete(AccessTokenCookieName);
        _httpContextAccessor?.HttpContext?.Response?.Cookies.Delete(RefreshTokenCookieName);
    }

    private CookieOptions CreateCookieOptions(int expiresInMinutes)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddMinutes(expiresInMinutes),
        };
    }
}
