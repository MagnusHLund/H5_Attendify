using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Features.Auth.Shared;
using Microsoft.AspNetCore.Identity;

namespace Attendify.Features.Auth.LoginWithPassword;

public sealed class LoginWithPasswordEndpoint(
    ApplicationDbContext dbContext,
    IAuthenticationSessionService authenticationSessionService,
    IPasswordHasher<User> passwordHasher
) : Endpoint<LoginWithPasswordRequest>
{
    private readonly ILogger _logger = Log.ForContext<LoginWithPasswordEndpoint>();

    public override void Configure()
    {
        Post("/login");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("LoginWithPassword"));
    }

    public override async Task HandleAsync(LoginWithPasswordRequest req, CancellationToken ct)
    {
        string email = EmailNormalizer.Normalize(req.Email);

        User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user == null)
        {
            await SendLoginFailedError(email, ct);
            return;
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            req.Password
        );

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            _logger.Warning("Login attempt failed: incorrect password");
            await SendLoginFailedError(email, ct);
            return;
        }

        _logger.Information("Login attempt succeeded");

        AuthenticationSession session = await authenticationSessionService.CreateSessionAsync(
            user,
            ct
        );
        authenticationSessionService.SetSessionCookies(session);

        await Send.NoContentAsync(cancellation: ct);
    }

    private async Task SendLoginFailedError(string email, CancellationToken ct)
    {
        AddError("Invalid email or password.");

        _logger.Warning("Login attempt failed: invalid email or password");

        await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
}
