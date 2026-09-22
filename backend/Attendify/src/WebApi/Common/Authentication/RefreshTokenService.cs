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
        byte[] tokenBytes = GenerateTokenBytes();
        byte[] tokenHash = HashToken(tokenBytes);

        RefreshToken refreshToken = RefreshToken.Create(
            userId,
            tokenHash,
            DateTimeOffset.UtcNow.AddDays(LifetimeDays),
            null
        );

        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Convert.ToBase64String(tokenBytes);
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
        DateTimeOffset now = DateTimeOffset.UtcNow;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        int revokedRows = await _dbContext
            .RefreshTokens.Where(refreshToken =>
                refreshToken.TokenHash == tokenHash
                && refreshToken.RevokedAt == null
                && refreshToken.ExpiresAt > now
            )
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(refreshToken => refreshToken.RevokedAt, now),
                cancellationToken
            );

        if (revokedRows != 1)
        {
            return null;
        }

        RefreshToken? existingToken = await _dbContext
            .RefreshTokens.Include(refreshToken => refreshToken.User)
            .SingleOrDefaultAsync(
                refreshToken => refreshToken.TokenHash == tokenHash,
                cancellationToken
            );

        if (existingToken is null)
        {
            return null;
        }

        byte[] newTokenBytes = GenerateTokenBytes();

        RefreshToken newRefreshToken = RefreshToken.Create(
            existingToken.UserId,
            HashToken(newTokenBytes),
            now.AddDays(LifetimeDays),
            null
        );

        _dbContext.RefreshTokens.Add(newRefreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

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
