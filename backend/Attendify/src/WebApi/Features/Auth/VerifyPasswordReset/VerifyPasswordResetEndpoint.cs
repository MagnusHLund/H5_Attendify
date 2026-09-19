namespace Attendify.Features.Auth.VerifyPasswordReset;

public sealed class VerifyPasswordResetEndpoint(ApplicationDbContext dbContext)
    : Endpoint<VerifyPasswordResetRequest>
{
    public override void Configure()
    {
        Post("/password-reset/verify");
        Group<AuthenticationGroup>();
        Description(x => x.WithName("VerifyPasswordReset"));
    }

    public override async Task HandleAsync(VerifyPasswordResetRequest req, CancellationToken ct) { }
}
