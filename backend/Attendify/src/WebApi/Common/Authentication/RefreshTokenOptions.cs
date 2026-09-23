using System.ComponentModel.DataAnnotations;

namespace Attendify.Common.Authentication;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    [Range(1, int.MaxValue)]
    public int LifetimeMinutes { get; set; } = 43200;
}
