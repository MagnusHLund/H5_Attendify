namespace Attendify.Features.Auth.Logout;

public sealed class LogoutEndpoint(ApplicationDbContext dbContext) : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/logout");
        Group<AuthenticationGroup>();
        Description(x => x.WithName("Logout"));
    }

    public override async Task HandleAsync(CancellationToken ct) { }
}
