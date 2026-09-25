using System.Security.Cryptography;
using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.Users;
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

    public async Task<GeneratedRefreshToken> GenerateRefreshToken(
        int userId,
        CancellationToken cancellationToken
    )
    {
        byte[] tokenBytes = GenerateTokenBytes();
        byte[] tokenHash = HashToken(tokenBytes);
        Guid tokenFamilyId = Guid.NewGuid();

        var expires = DateTimeOffset.UtcNow.AddMinutes(_refreshTokenOptions.LifetimeMinutes);

        RefreshToken refreshToken = RefreshToken.Create(
            userId,
            tokenFamilyId,
            tokenHash,
            expires,
            null
        );

        _dbContext.RefreshTokens.Add(refreshToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new GeneratedRefreshToken(Convert.ToBase64String(tokenBytes), tokenFamilyId);
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

        var tokenFamily = await _dbContext.RefreshTokens
            .AsNoTracking()
            .Where(refreshToken => refreshToken.TokenHash == tokenHash)
            .Select(refreshToken => new
            {
                refreshToken.UserId,
                refreshToken.TokenFamilyId,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (tokenFamily is null)
            return null;

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            _dbContext.ChangeTracker.Clear();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );

            await LockUserRowAsync(tokenFamily.UserId, cancellationToken);

            int revokedRows = await _dbContext
                .RefreshTokens.Where(refreshToken =>
                    refreshToken.TokenHash == tokenHash
                    && refreshToken.UserId == tokenFamily.UserId
                    && refreshToken.TokenFamilyId == tokenFamily.TokenFamilyId
                    && refreshToken.RevokedAt == null
                    && refreshToken.ExpiresAt > now
                )
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(refreshToken => refreshToken.RevokedAt, now),
                    cancellationToken
                );

            if (revokedRows != 1)
                return null;

            User? user = await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(
                candidate => candidate.Id == tokenFamily.UserId,
                cancellationToken
            );

            if (user is null)
                return null;

            byte[] newTokenBytes = GenerateTokenBytes();
            RefreshToken newRefreshToken = RefreshToken.Create(
                tokenFamily.UserId,
                tokenFamily.TokenFamilyId,
                HashToken(newTokenBytes),
                now.AddMinutes(_refreshTokenOptions.LifetimeMinutes),
                null
            );

            _dbContext.RefreshTokens.Add(newRefreshToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new RotatedRefreshTokenResult(
                Convert.ToBase64String(newTokenBytes),
                tokenFamily.TokenFamilyId,
                user
            );
        });
    }

    public async Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken)
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

        var tokenFamily = await _dbContext.RefreshTokens
            .AsNoTracking()
            .Where(refreshToken => refreshToken.TokenHash == tokenHash)
            .Select(refreshToken => new
            {
                refreshToken.UserId,
                refreshToken.TokenFamilyId,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (tokenFamily is null)
            return false;

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            _dbContext.ChangeTracker.Clear();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken
            );

            await LockUserRowAsync(tokenFamily.UserId, cancellationToken);

            int revokedRows = await _dbContext.RefreshTokens
                .Where(refreshToken =>
                    refreshToken.TokenFamilyId == tokenFamily.TokenFamilyId
                    && refreshToken.RevokedAt == null
                )
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(refreshToken => refreshToken.RevokedAt, now),
                    cancellationToken
                );

            await transaction.CommitAsync(cancellationToken);

            return revokedRows > 0;
        });
    }

    public Task<bool> IsTokenFamilyActiveAsync(
        Guid tokenFamilyId,
        CancellationToken cancellationToken
    )
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return _dbContext.RefreshTokens.AnyAsync(
            refreshToken =>
                refreshToken.TokenFamilyId == tokenFamilyId
                && refreshToken.RevokedAt == null
                && refreshToken.ExpiresAt > now,
            cancellationToken
        );
    }

    private async Task LockUserRowAsync(int userId, CancellationToken cancellationToken)
    {
        await _dbContext.Users
            .FromSqlInterpolated($"SELECT * FROM \"Users\" WHERE \"Id\" = {userId} FOR UPDATE")
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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
