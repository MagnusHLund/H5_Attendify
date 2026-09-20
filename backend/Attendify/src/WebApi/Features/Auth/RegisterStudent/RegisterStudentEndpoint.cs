namespace Attendify.Features.Auth.RegisterStudent;

public sealed class RegisterStudentEndpoint(ApplicationDbContext dbContext)
    : Endpoint<RegisterStudentRequest>
{
    public override void Configure()
    {
        Post("/register");
        Group<AuthenticationGroup>();
        Description(x => x.WithName("RegisterStudent"));
    }

    public override async Task HandleAsync(RegisterStudentRequest req, CancellationToken ct) { }
}
