namespace Attendify.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserRequest(ApplicationDbContext dbContext)
    : Endpoint<GetCurrentUserRequest>
{
    public override void Configure()
    {
        Get("/me");
        Group<AuthenticationGroup>();
        Description(x => x.WithName("GetCurrentUser"));
    }

    public override async Task HandleAsync(GetCurrentUserRequest req, CancellationToken ct) { }
}
