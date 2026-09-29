using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Common.Domain.Authentication;
using studentAccessCodeClass = Attendify.Common.Domain.Authentication;

namespace Attendify.Features.Auth.LoginWithAccessCode;

public sealed class LoginWithAccessCodeEndpoint(
        ApplicationDbContext dbContext,
        IStudentAccessCodeGenerator accessCodeGenerator,
        IAuthenticationSessionService sessionService,
        IAuthenticationCookieService cookieService
)
    : Endpoint<LoginWithAccessCodeRequest>
{
    public override void Configure()
    {
        Post("/login/access-code");
        Group<AuthenticationGroup>();
        AllowAnonymous();
        Description(x => x.WithName("LoginWithAccessCode"));
    }

    public override async Task HandleAsync(LoginWithAccessCodeRequest req, CancellationToken ct)
    {
        string submittedCodeHash = accessCodeGenerator.HashCode(req.StudentAccessCode);

        StudentAccessCode? accessCode = await dbContext.StudentAccessCodes
            .Include(x => x.User)
            .SingleOrDefaultAsync(
                x =>
                    x.AccessCodeHash == submittedCodeHash &&
                    x.ExpiresAt > DateTimeOffset.UtcNow,
                ct);

        if (accessCode is null)
        {
            AddError(studentAccessCodeClass.StudentAccessCodeErrors.Invalid.Description);

            await Send.ErrorsAsync(
                StatusCodes.Status401Unauthorized,
                ct);

            return;
        }

        AdministrativeSession session = await sessionService.CreateAdministrativeSessionAsync(accessCode.User, ct);

        cookieService.SetAccessTokenCookie(session.AccessToken);

        await Send.NoContentAsync(cancellation: ct);

    }
}