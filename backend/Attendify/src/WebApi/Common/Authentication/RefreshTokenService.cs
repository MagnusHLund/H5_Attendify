using System.Security.Cryptography;
using Attendify.Common.Domain.Authentication;

namespace Attendify.Common.Authentication;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private const int TokenSizeBytes = 64;
    private const int LifetimeDays = 30;

    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateRefreshToken(int userId, CancellationToken cancellationToken)
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeBytes);

        string token = Convert.ToBase64String(tokenBytes);
        byte[] hashedToken = HashToken(tokenBytes);

        RefreshToken refreshToken = RefreshToken.Create(
            userId,
            hashedToken,
            DateTime.UtcNow.AddDays(LifetimeDays)
        );

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task<RotatedRefreshTokenResult?> RotateTokenAsync(
        string token,
        CancellationToken cancellationToken
    )
    {
        byte[] tokenBytes;

        try
        {
            tokenBytes = Convert.FromBase64String(token);
        }
        catch (FormatException)
        {
            return null;
        }

        byte[] tokenHash = HashToken(tokenBytes);

        RefreshToken? existingToken = await _dbContext
            .RefreshTokens.Include(refreshToken => refreshToken.User)
            .SingleOrDefaultAsync(
                refreshToken => refreshToken.TokenHash.SequenceEqual(tokenHash),
                cancellationToken
            );

        if (existingToken is null)
        {
            return null;
        }

        if (existingToken.RevokedAt.HasValue || existingToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        existingToken.Revoke();

        byte[] newTokenBytes = GenerateTokenBytes();

        RefreshToken newRefreshToken = RefreshToken.Create(
            existingToken.UserId,
            HashToken(newTokenBytes),
            DateTimeOffset.UtcNow.AddDays(LifetimeDays),
            null
        );

        _dbContext.RefreshTokens.Add(newRefreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RotatedRefreshTokenResult(
            Convert.ToBase64String(newTokenBytes),
            existingToken.User
        );
    }

    private static byte[] HashToken(byte[] token)
    {
        return SHA256.HashData(token);
    }

    private static byte[] GenerateTokenBytes()
    {
        return RandomNumberGenerator.GetBytes(TokenSizeBytes);
    }
}
