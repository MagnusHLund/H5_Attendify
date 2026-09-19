namespace Attendify.Features.Auth.CompletePasswordReset;

public sealed class CompletePasswordResetEndpoint(ApplicationDbContext dbContext)
    : Endpoint<CompletePasswordResetRequest>
{
    public override void Configure()
    {
        Post("/password-reset/complete");
        Group<AuthenticationGroup>();
        Description(x => x.WithName("CompletePasswordReset"));
    }

    public override async Task HandleAsync(
        CompletePasswordResetRequest req,
        CancellationToken ct
    ) { }
}
