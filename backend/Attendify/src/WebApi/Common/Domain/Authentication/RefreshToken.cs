using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Users;

namespace Attendify.Common.Domain.Authentication;

public sealed class RefreshToken : AggregateRoot<int>
{
    public const int TokenHashMaxLength = 512;

    public int UserId { get; set; }

    public string TokenHash
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(TokenHash));
            ThrowIfGreaterThan(value.Length, TokenHashMaxLength, nameof(TokenHash));
            field = value;
        }
    } = null!;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public User User { get; set; } = null!;

    private RefreshToken() { }

    public static RefreshToken Create(
        int userId,
        string tokenHash,
        DateTimeOffset expiresAt,
        DateTimeOffset? revokedAt
    )
    {
        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            RevokedAt = revokedAt,
        };
    }
}
