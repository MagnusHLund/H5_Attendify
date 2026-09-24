using System.ComponentModel.DataAnnotations;

namespace Attendify.Common.Authentication;

public sealed class ResetPasswordTokenOptions
{
    public const string SectionName = "ResetPasswordToken";

    [Range(5, 60)]
    public int LifetimeMinutes { get; set; } = 15;

    [Required]
    public string SecurityCodeHashKey { get; set; } = string.Empty;

    public bool HasValidSecurityCodeHashKey()
    {
        if (string.IsNullOrWhiteSpace(SecurityCodeHashKey))
            return false;

        try
        {
            return Convert.FromBase64String(SecurityCodeHashKey).Length == 32;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
