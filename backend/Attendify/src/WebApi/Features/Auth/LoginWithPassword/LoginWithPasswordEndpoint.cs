namespace Attendify.Features.Auth.LoginWithPassword;

public sealed class LoginWithPasswordEndpoint(ApplicationDbContext dbContext)
    : Endpoint<LoginWithPasswordRequest>
{
    public override void Configure()
    {
        Post("/login");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("LoginWithPassword"));
    }

    public override async Task HandleAsync(LoginWithPasswordRequest req, CancellationToken ct) { }
}
