using System.Security.Cryptography;
using System.Text;
using Attendify.Common.Authentication;
using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Common.Email;
using Attendify.Features.Auth.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Attendify.Features.Auth.PasswordReset;

public sealed class PasswordResetService : IPasswordResetService
{
    private const string CodeAlphabet = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    private readonly ApplicationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ResetPasswordTokenOptions _options;
    private readonly byte[] _securityCodeHashKey;
    private readonly TimeProvider _timeProvider;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger _logger = Log.ForContext<PasswordResetService>();

    public PasswordResetService(
        ApplicationDbContext dbContext,
        IEmailSender emailSender,
        IOptions<ResetPasswordTokenOptions> options,
        TimeProvider timeProvider,
        IPasswordHasher<User> passwordHasher
    )
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _options = options.Value;
        _securityCodeHashKey = Convert.FromBase64String(_options.SecurityCodeHashKey);
        _timeProvider = timeProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken)
    {
        string normalizedEmail = EmailNormalizer.Normalize(email);
        User? user = await FindUserByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return;
        }

        string securityCode = GenerateSecurityCode();
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.AddMinutes(_options.LifetimeMinutes);
        byte[] securityCodeHash = HashSecurityCode(user.Id, securityCode);

        PasswordResetToken? resetToken = await _dbContext.PasswordResetTokens.SingleOrDefaultAsync(
            token => token.UserId == user.Id,
            cancellationToken
        );

        if (resetToken is null)
        {
            _dbContext.PasswordResetTokens.Add(
                PasswordResetToken.Create(user.Id, securityCodeHash, expiresAt)
            );
        }
        else
        {
            resetToken.ReplaceCode(securityCodeHash, expiresAt);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            OutboundEmail outboundEmail = PasswordResetEmailTemplate.Create(
                normalizedEmail,
                securityCode,
                _options.LifetimeMinutes
            );

            await _emailSender.SendAsync(outboundEmail, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Keep the public response identical for existing and unknown accounts.
            // The user can request another code if delivery fails.
            _logger.Error(
                exception,
                "Failed to send a password reset email for user {UserId}.",
                user.Id
            );
        }
    }

    public async Task<bool> VerifyPasswordResetAsync(
        string email,
        string securityCode,
        CancellationToken cancellationToken
    )
    {
        PasswordResetContext? context = await FindPasswordResetContextAsync(email, cancellationToken);
        if (context?.Token is not { } resetToken || !IsUsable(resetToken))
            return false;

        if (!IsSecurityCodeMatch(context.UserId, resetToken, securityCode))
        {
            await RecordFailedAttemptAsync(resetToken, cancellationToken);
            return false;
        }

        return true;
    }

    public async Task<bool> CompleteResetPasswordAsync(
        string email,
        string securityCode,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        PasswordResetContext? context = await FindPasswordResetContextAsync(email, cancellationToken);
        if (context?.Token is not { } resetToken || !IsUsable(resetToken))
            return false;

        if (!IsSecurityCodeMatch(context.UserId, resetToken, securityCode))
        {
            await RecordFailedAttemptAsync(resetToken, cancellationToken);
            return false;
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(() => CompletePasswordResetAttemptAsync(
            context.UserId,
            resetToken,
            newPassword,
            _timeProvider.GetUtcNow(),
            cancellationToken
        ));
    }

    private async Task<User?> FindUserByEmailAsync(string email, CancellationToken cancellationToken) =>
        await _dbContext.Users.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.Email == email,
            cancellationToken
        );

    private async Task<PasswordResetContext?> FindPasswordResetContextAsync(
        string email,
        CancellationToken cancellationToken
    )
    {
        string normalizedEmail = EmailNormalizer.Normalize(email);
        User? user = await FindUserByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
            return null;

        PasswordResetToken? token = await _dbContext.PasswordResetTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.UserId == user.Id, cancellationToken);

        return new PasswordResetContext(user.Id, token);
    }

    private bool IsUsable(PasswordResetToken token) => token.IsUsableAt(_timeProvider.GetUtcNow());

    private bool IsSecurityCodeMatch(
        int userId,
        PasswordResetToken resetToken,
        string securityCode
    ) => CryptographicOperations.FixedTimeEquals(
        resetToken.SecurityCodeHash,
        HashSecurityCode(userId, securityCode)
    );

    private async Task<bool> CompletePasswordResetAttemptAsync(
        int userId,
        PasswordResetToken resetToken,
        string newPassword,
        DateTimeOffset now,
        CancellationToken cancellationToken
    )
    {
        // The execution strategy can retry the entire unit. Start each attempt
        // with fresh tracked state, and consume the code conditionally in SQL.
        _dbContext.ChangeTracker.Clear();

        User? user = await _dbContext.Users.SingleOrDefaultAsync(
            candidate => candidate.Id == userId,
            cancellationToken
        );

        if (user is null)
            return false;

        string hashedPassword = _passwordHasher.HashPassword(user, newPassword);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            cancellationToken
        );

        if (!await TryConsumeResetTokenAsync(userId, resetToken, now, cancellationToken))
            return false;

        user.UpdatePassword(hashedPassword);
        await RevokeRefreshTokensAsync(userId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    private async Task<bool> TryConsumeResetTokenAsync(
        int userId,
        PasswordResetToken resetToken,
        DateTimeOffset now,
        CancellationToken cancellationToken
    )
    {
        int deletedRows = await _dbContext.PasswordResetTokens
            .Where(token =>
                token.Id == resetToken.Id
                && token.UserId == userId
                && token.SecurityCodeHash == resetToken.SecurityCodeHash
                && token.ConsumedAt == null
                && token.FailedAttempts < PasswordResetToken.MaxFailedAttempts
                && token.ExpiresAt > now
            )
            .ExecuteDeleteAsync(cancellationToken);

        return deletedRows == 1;
    }

    private async Task RevokeRefreshTokensAsync(int userId, CancellationToken cancellationToken) =>
        await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);

    private async Task RecordFailedAttemptAsync(
        PasswordResetToken resetToken,
        CancellationToken cancellationToken
    )
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();

        await _dbContext.PasswordResetTokens
            .Where(token =>
                token.Id == resetToken.Id
                && token.SecurityCodeHash == resetToken.SecurityCodeHash
                && token.ConsumedAt == null
                && token.FailedAttempts < PasswordResetToken.MaxFailedAttempts
                && token.ExpiresAt > now
            )
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    token => token.FailedAttempts,
                    token => token.FailedAttempts + 1
                ),
                cancellationToken
            );
    }

    private byte[] HashSecurityCode(int userId, string securityCode)
    {
        byte[] value = Encoding.UTF8.GetBytes($"{userId}:{securityCode}");
        return HMACSHA256.HashData(_securityCodeHashKey, value);
    }

    private static string GenerateSecurityCode()
    {
        Span<char> code = stackalloc char[9];

        for (int index = 0; index < code.Length; index++)
            code[index] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];

        return new string(code);
    }
}
