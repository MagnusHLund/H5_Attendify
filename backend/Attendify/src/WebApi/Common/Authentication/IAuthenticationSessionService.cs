using Attendify.Common.Domain.Users;

namespace Attendify.Common.Authentication;

public interface IAuthenticationSessionService
{
    Task CreateSessionAsync(User user, CancellationToken cancellationToken);
    Task<bool> RefreshSessionAsync(string refreshToken, CancellationToken cancellationToken);
}
