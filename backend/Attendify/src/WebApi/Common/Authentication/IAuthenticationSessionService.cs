using Attendify.Common.Domain.Users;

namespace Attendify.Common.Authentication;

public interface IAuthenticationSessionService
{
    Task<AuthenticationSession> CreateSessionAsync(
        User user,
        CancellationToken cancellationToken
    );
    Task<bool> IsPersistedAsync(
        User user,
        AuthenticationSession session,
        CancellationToken cancellationToken
    );
    void SetSessionCookies(AuthenticationSession session);
    Task<bool> RefreshSessionAsync(string refreshToken, CancellationToken cancellationToken);
}

public sealed record AuthenticationSession(string AccessToken, string RefreshToken);
