using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Users;

namespace Attendify.Common.Domain.Authentication;

public sealed class RefreshToken : AggregateRoot<int>
{
    public const int TokenHashLength = 32;

    public int UserId { get; set; }

    public Guid TokenFamilyId { get; set; }

    public byte[] TokenHash
    {
        get;
        set
        {
            ThrowIfNull(value, nameof(TokenHash));
            ThrowIfNotEqual(value.Length, TokenHashLength, nameof(TokenHash));
            field = value;
        }
    } = null!;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public User User { get; set; } = null!;

    private RefreshToken() { }

    public static RefreshToken Create(
        int userId,
        Guid tokenFamilyId,
        byte[] tokenHash,
        DateTimeOffset expiresAt,
        DateTimeOffset? revokedAt = null
    )
    {
        return new RefreshToken
        {
            UserId = userId,
            TokenFamilyId = tokenFamilyId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            RevokedAt = revokedAt,
        };
    }

    public void Revoke()
    {
        RevokedAt = DateTimeOffset.UtcNow;
    }
}
