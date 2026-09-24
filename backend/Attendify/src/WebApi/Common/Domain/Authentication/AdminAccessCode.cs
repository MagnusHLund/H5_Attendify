using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Users;

namespace Attendify.Common.Domain.Authentication;

public sealed class AdminAccessCode : AggregateRoot<int>
{
    public const int CodeMaxLength = 128;

    public int UserId { get; set; }

    public string EncryptedAccessCode
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(EncryptedAccessCode));
            ThrowIfGreaterThan(value.Length, CodeMaxLength, nameof(EncryptedAccessCode));
            field = value;
        }
    } = null!;

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public User User { get; set; } = null!;

    private AdminAccessCode() { }

    public static AdminAccessCode Create(
        int userId,
        string code,
        DateTimeOffset generatedAt,
        DateTimeOffset expiresAt
    )
    {
        return new AdminAccessCode
        {
            UserId = userId,
            EncryptedAccessCode = code,
            GeneratedAt = generatedAt,
            ExpiresAt = expiresAt,
        };
    }
}
