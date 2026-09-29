using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Users;

namespace Attendify.Common.Domain.Authentication;

public sealed class StudentAccessCode : AggregateRoot<int>
{
    public const int CodeMaxLength = 128;

    public int UserId { get; set; }

    public string AccessCodeHash
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(AccessCodeHash));
            ThrowIfGreaterThan(value.Length, CodeMaxLength, nameof(AccessCodeHash));
            field = value;
        }
    } = null!;

    public DateTimeOffset GeneratedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateOnly GenerationDate { get; set; }

    public User User { get; set; } = null!;

    private StudentAccessCode() { }

    public static StudentAccessCode Create(
        int userId,
        string code,
        DateTimeOffset generatedAt,
        DateOnly generationDate,
        DateTimeOffset expiresAt
    )
    {
        return new StudentAccessCode
        {
            UserId = userId,
            AccessCodeHash = code,
            GeneratedAt = generatedAt,
            GenerationDate = generationDate,
            ExpiresAt = expiresAt
        };
    }
}