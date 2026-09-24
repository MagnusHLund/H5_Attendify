namespace Attendify.Features.Auth.VerifyPasswordReset;

public sealed record VerifyPasswordResetRequest(string Email, string SecurityCode);
