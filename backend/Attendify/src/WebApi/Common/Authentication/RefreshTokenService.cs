using System.Security.Cryptography;
using Attendify.Common.Domain.Authentication;
using Microsoft.Extensions.Options;

namespace Attendify.Common.Authentication;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private const int TokenSizeBytes = 64;

    private readonly RefreshTokenOptions _refreshTokenOptions;

    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenService(
        ApplicationDbContext dbContext,
        IOptions<RefreshTokenOptions> refreshTokenOptions
    )
    {
        _dbContext = dbContext;
        _refreshTokenOptions = refreshTokenOptions.Value;
    }

    public async Task<string> GenerateRefreshToken(int userId, CancellationToken cancellationToken)
    {
        byte[] tokenBytes = GenerateTokenBytes();
        byte[] tokenHash = HashToken(tokenBytes);

        var expires = DateTimeOffset.UtcNow.AddMinutes(_refreshTokenOptions.LifetimeMinutes);

        RefreshToken refreshToken = RefreshToken.Create(userId, tokenHash, expires, null);

        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Convert.ToBase64String(tokenBytes);
    }

    public async Task<bool> IsPersistedAsync(
        int userId,
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
            return false;
        }

        byte[] tokenHash = HashToken(tokenBytes);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return await _dbContext.RefreshTokens.AnyAsync(
            refreshToken =>
                refreshToken.UserId == userId
                && refreshToken.TokenHash == tokenHash
            && refreshToken.RevokedAt == null
                && refreshToken.ExpiresAt > now,
            cancellationToken
        );
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
            now.AddMinutes(_refreshTokenOptions.LifetimeMinutes),
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
