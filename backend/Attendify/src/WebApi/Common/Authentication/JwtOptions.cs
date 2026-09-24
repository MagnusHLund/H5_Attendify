using System.ComponentModel.DataAnnotations;

namespace Attendify.Common.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const string AuthenticatedUserPolicy = "AuthenticatedUser";

    [Required]
    public string Issuer { get; init; } = null!;

    [Required]
    public string Audience { get; init; } = null!;

    [Required]
    [MinLength(32)]
    public string SigningKey { get; init; } = null!;

    [Range(1, 60)]
    public int AccessTokenLifetimeMinutes { get; init; } = 15;
}
