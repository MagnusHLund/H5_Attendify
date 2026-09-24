namespace Attendify.Features.Auth.RequestPasswordReset;

public static class PasswordResetRequestRateLimit
{
    public const string PolicyName = "PasswordResetRequest";
    public const int PermitLimit = 5;
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);
}
