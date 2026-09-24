namespace Attendify.Features.Auth.PasswordReset;

public interface IPasswordResetRequestQueue
{
    ValueTask EnqueueAsync(string email, CancellationToken cancellationToken);
}
