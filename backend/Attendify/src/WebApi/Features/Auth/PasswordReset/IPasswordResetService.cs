namespace Attendify.Features.Auth.PasswordReset;

public interface IPasswordResetService
{
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken);
}
