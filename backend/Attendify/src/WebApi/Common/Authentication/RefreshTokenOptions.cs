namespace Attendify.Common.Authentication;

public class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int LifetimeMinutes { get; set; } = 43200;
}
