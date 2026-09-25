namespace Attendify.Features.Auth.PasswordReset;

public interface IPasswordResetService
{
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken);
    Task<bool> VerifyPasswordResetAsync(
        string email,
        string securityCode,
        CancellationToken cancellationToken
    );

    Task<bool> CompleteResetPasswordAsync(
        string email,
        string securityCode,
        string newPassword,
        CancellationToken cancellationToken
    );
}
