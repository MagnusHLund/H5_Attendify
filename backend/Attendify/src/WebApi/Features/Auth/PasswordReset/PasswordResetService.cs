using System.Security.Cryptography;
using System.Text;
using Attendify.Common.Authentication;
using Attendify.Common.Domain.Authentication;
using Attendify.Common.Email;
using Attendify.Features.Auth.Shared;
using Microsoft.EntityFrameworkCore;
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
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        ApplicationDbContext dbContext,
        IEmailSender emailSender,
        IOptions<ResetPasswordTokenOptions> options,
        TimeProvider timeProvider,
        ILogger<PasswordResetService> logger
    )
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _options = options.Value;
        _securityCodeHashKey = Convert.FromBase64String(_options.SecurityCodeHashKey);
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken)
    {
        string normalizedEmail = EmailNormalizer.Normalize(email);

        var user = await _dbContext.Users.SingleOrDefaultAsync(
            candidate => candidate.Email == normalizedEmail,
            cancellationToken
        );

        if (user is null)
        {
            await Task.Delay(600, cancellationToken);
            return;
        }

        string securityCode = GenerateSecurityCode();
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.AddMinutes(_options.ExpiresInMinutes);
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
                _options.ExpiresInMinutes
            );

            await _emailSender.SendAsync(outboundEmail, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Keep the public response identical for existing and unknown accounts.
            // The user can request another code if delivery fails.
            _logger.LogError(
                exception,
                "Failed to send a password reset email for user {UserId}.",
                user.Id
            );
        }
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
