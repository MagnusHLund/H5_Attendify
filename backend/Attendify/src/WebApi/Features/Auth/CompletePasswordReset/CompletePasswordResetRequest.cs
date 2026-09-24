namespace Attendify.Features.Auth.CompletePasswordReset;

public sealed record CompletePasswordResetRequest(string SecurityCode, string NewPassword);
