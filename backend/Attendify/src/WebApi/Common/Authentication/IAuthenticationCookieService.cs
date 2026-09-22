namespace Attendify.Common.Authentication;

public interface IAuthenticationCookieService
{
    void SetAccessTokenCookie(string accessToken);
    void SetRefreshTokenCookie(string refreshToken);
    void ClearAuthenticationCookies();
}
