using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Users;

namespace Attendify.Common.Domain.Authentication;

public sealed class PasswordResetToken : AggregateRoot<int>
{
    public const int SecurityCodeHashLength = 32;
    public const int MaxFailedAttempts = 3;

    public int UserId { get; set; }

    public byte[] SecurityCodeHash
    {
        get;
        set
        {
            ThrowIfNull(value, nameof(SecurityCodeHash));
            ThrowIfNotEqual(value.Length, SecurityCodeHashLength, nameof(SecurityCodeHash));
            field = value;
        }
    } = null!;

    public DateTimeOffset ExpiresAt { get; set; }

    public int FailedAttempts { get; private set; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    public User User { get; set; } = null!;

    private PasswordResetToken() { }

    public static PasswordResetToken Create(
        int userId,
        byte[] securityCodeHash,
        DateTimeOffset expiresAt
    ) =>
        new()
        {
            UserId = userId,
            SecurityCodeHash = securityCodeHash,
            ExpiresAt = expiresAt,
        };

    public bool IsUsableAt(DateTimeOffset now) =>
        ConsumedAt is null && FailedAttempts < MaxFailedAttempts && ExpiresAt > now;

    public void ReplaceCode(byte[] securityCodeHash, DateTimeOffset expiresAt)
    {
        SecurityCodeHash = securityCodeHash;
        ExpiresAt = expiresAt;
        FailedAttempts = 0;
        ConsumedAt = null;
    }

    public void RecordFailedAttempt()
    {
        if (FailedAttempts < MaxFailedAttempts)
            FailedAttempts++;
    }

    public void Consume(DateTimeOffset consumedAt)
    {
        ConsumedAt ??= consumedAt;
    }
}
