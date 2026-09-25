namespace Attendify.Common.Authentication;

public interface IRefreshTokenService
{
    Task<GeneratedRefreshToken> GenerateRefreshToken(
        int userId,
        CancellationToken cancellationToken
    );
    Task<bool> IsPersistedAsync(
        int userId,
        string token,
        CancellationToken cancellationToken
    );
    Task<RotatedRefreshTokenResult?> RotateTokenAsync(
        string token,
        CancellationToken cancellationToken
    );
    Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken);
    Task<bool> IsTokenFamilyActiveAsync(Guid tokenFamilyId, CancellationToken cancellationToken);
}
