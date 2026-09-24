using Attendify.Features.Auth.PasswordReset;

namespace Attendify.Features.Auth.VerifyPasswordReset;

public sealed class VerifyPasswordResetEndpoint(IPasswordResetService resetPasswordService)
    : Endpoint<VerifyPasswordResetRequest>
{
    public override void Configure()
    {
        Post("/password-reset/verify");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("VerifyPasswordReset"));
    }

    public override async Task HandleAsync(VerifyPasswordResetRequest req, CancellationToken ct)
    {
        bool success = await resetPasswordService.VerifyPasswordResetAsync(
            req.Email,
            req.SecurityCode,
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
