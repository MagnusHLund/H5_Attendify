namespace Attendify.Common.Authentication;

public interface IRefreshTokenService
{
    Task<string> GenerateRefreshToken(int userId, CancellationToken cancellationToken);
    Task<RotatedRefreshTokenResult?> RotateTokenAsync(
        string token,
        CancellationToken cancellationToken
    );
}
