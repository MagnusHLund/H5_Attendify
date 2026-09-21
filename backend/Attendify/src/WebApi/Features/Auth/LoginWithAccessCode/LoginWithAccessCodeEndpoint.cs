namespace Attendify.Features.Auth.LoginWithAccessCode;

public sealed class LoginWithAccessCodeEndpoint(ApplicationDbContext dbContext)
    : Endpoint<LoginWithAccessCodeRequest>
{
    public override void Configure()
    {
        Post("/login/access-code");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("LoginWithAccessCode"));
    }

    public override async Task HandleAsync(LoginWithAccessCodeRequest req, CancellationToken ct) { }
}
