namespace Attendify.Common.Authentication;

public class AuthenticationCookieService : IAuthenticationCookieService
{
    public const string AccessTokenCookieName = "AccessToken";
    public const string RefreshTokenCookieName = "RefreshToken";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationCookieService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetAccessTokenCookie(string accessToken)
    {
        _httpContextAccessor?.HttpContext?.Response?.Cookies.Append(
            AccessTokenCookieName,
            accessToken,
            CreateCookieOptions()
        );
    }

    public void SetRefreshTokenCookie(string refreshToken)
    {
        _httpContextAccessor?.HttpContext?.Response?.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            CreateCookieOptions()
        );
    }

    public void ClearAuthenticationCookies()
    {
        _httpContextAccessor?.HttpContext?.Response?.Cookies.Delete(AccessTokenCookieName);
        _httpContextAccessor?.HttpContext?.Response?.Cookies.Delete(RefreshTokenCookieName);
    }

    private CookieOptions CreateCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
        };
    }
}
