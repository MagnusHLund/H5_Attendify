using System.Security.Claims;
using Attendify.Common.Domain.Users;
using Attendify.Common.Services;

namespace Attendify.Common.Authentication;

public sealed class AuthenticationSessionService : IAuthenticationSessionService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IStudentIdProtector _studentIdProtector;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuthenticationCookieService _authenticationCookieService;

    public AuthenticationSessionService(
        IJwtTokenService jwtTokenService,
        IStudentIdProtector studentIdProtector,
        IRefreshTokenService refreshTokenService,
        IAuthenticationCookieService authenticationCookieService
    )
    {
        _jwtTokenService = jwtTokenService;
        _studentIdProtector = studentIdProtector;
        _refreshTokenService = refreshTokenService;
        _authenticationCookieService = authenticationCookieService;
    }

    public async Task<AuthenticationSession> CreateSessionAsync(
        User user,
        CancellationToken cancellationToken
    )
    {
        GeneratedRefreshToken refreshToken = await _refreshTokenService.GenerateRefreshToken(
            user.Id,
            cancellationToken
        );
        string accessToken = _jwtTokenService.GenerateToken(
            CreateClaims(user, refreshToken.TokenFamilyId)
        );

        return new AuthenticationSession(accessToken, refreshToken.Token);
    }

    public void SetSessionCookies(AuthenticationSession session)
    {
        _authenticationCookieService.SetAccessTokenCookie(session.AccessToken);
        _authenticationCookieService.SetRefreshTokenCookie(session.RefreshToken);
    }

    public Task<bool> IsPersistedAsync(
        User user,
        AuthenticationSession session,
        CancellationToken cancellationToken
    )
    {
        return _refreshTokenService.IsPersistedAsync(
            user.Id,
            session.RefreshToken,
            cancellationToken
        );
    }

    public async Task<bool> RefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken
    )
    {
        var result = await _refreshTokenService.RotateTokenAsync(refreshToken, cancellationToken);

        if (result is null)
        {
            _authenticationCookieService.ClearAuthenticationCookies();
            return false;
        }

        var claims = CreateClaims(result.User, result.TokenFamilyId);
        string accessToken = _jwtTokenService.GenerateToken(claims);

        _authenticationCookieService.SetAccessTokenCookie(accessToken);
        _authenticationCookieService.SetRefreshTokenCookie(result.Token);

        return true;
    }

    private List<Claim> CreateClaims(User user, Guid tokenFamilyId)
    {
        string decryptedStudentId = _studentIdProtector.Unprotect(user.EncryptedStudentId);

        return new List<Claim>
        {
            new Claim(AttendifyClaimTypes.UserId, user.Id.ToString()),
            new Claim(AttendifyClaimTypes.UserType, UserType.Student.ToString()),
            new Claim(AttendifyClaimTypes.StudentId, decryptedStudentId),
            new Claim(AttendifyClaimTypes.TokenFamilyId, tokenFamilyId.ToString()),
        };
    }
}
