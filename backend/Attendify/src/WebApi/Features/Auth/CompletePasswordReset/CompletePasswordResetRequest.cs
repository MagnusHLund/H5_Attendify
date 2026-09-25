namespace Attendify.Features.Auth.CompletePasswordReset;

public sealed record CompletePasswordResetRequest(
    string Email,
    string SecurityCode,
    string NewPassword
);
