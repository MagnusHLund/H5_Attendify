using Attendify.Features.Auth.PasswordReset;

namespace Attendify.Features.Auth.CompletePasswordReset;

public sealed class CompletePasswordResetEndpoint(IPasswordResetService resetPasswordService)
    : Endpoint<CompletePasswordResetRequest>
{
    public override void Configure()
    {
        Post("/password-reset/complete");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("CompletePasswordReset"));
    }

    public override async Task HandleAsync(CompletePasswordResetRequest req, CancellationToken ct)
    {
        bool success = await resetPasswordService.CompleteResetPasswordAsync(
            req.Email,
            req.SecurityCode,
            req.NewPassword,
            ct
        );

        if (success)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        await Send.ErrorsAsync(cancellation: ct);
    }
}
