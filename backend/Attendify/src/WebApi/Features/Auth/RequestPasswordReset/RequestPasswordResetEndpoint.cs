namespace Attendify.Features.Auth.RequestPasswordReset;

public sealed class RequestPasswordResetEndpoint(ApplicationDbContext dbContext)
    : Endpoint<RequestPasswordResetRequest>
{
    public override void Configure()
    {
        Post("/password-reset/request");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("RequestPasswordReset"));
    }

    public override async Task HandleAsync(
        RequestPasswordResetRequest req,
        CancellationToken ct
    ) { }
}
