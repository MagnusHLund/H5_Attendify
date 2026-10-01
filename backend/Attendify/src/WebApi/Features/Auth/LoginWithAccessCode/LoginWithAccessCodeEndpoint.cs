using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Common.Domain.Authentication;
using studentAccessCodeClass = Attendify.Common.Domain.Authentication;

namespace Attendify.Features.Auth.LoginWithAccessCode;

public sealed class LoginWithAccessCodeEndpoint(
        ApplicationDbContext dbContext,
        IStudentAccessCodeGenerator accessCodeGenerator,
        IAuthenticationSessionService sessionService,
        IAuthenticationCookieService cookieService,
        IServiceScopeFactory scopeFactory
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

        var strategy = dbContext.Database.CreateExecutionStrategy();
        AdministrativeSession? session = await strategy.ExecuteAsync(async () =>
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            ApplicationDbContext attempt = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await using var transaction = await attempt.Database.BeginTransactionAsync(ct);

            int? userId = await FindValidCodeOwnerAsync(attempt, submittedCodeHash, ct);
            if (userId is null || !await ActiveUserLock.AcquireAsync(attempt, userId.Value, ct))
                return null;

            // Account deletion removes access codes under the same lock, so re-check after acquiring it.
            if (await FindValidCodeOwnerAsync(attempt, submittedCodeHash, ct) != userId)
                return null;

            User user = await attempt.Users.SingleAsync(candidate => candidate.Id == userId.Value, ct);
            AdministrativeSession createdSession =
                await sessionService.CreateAdministrativeSessionAsync(user, ct);

            await transaction.CommitAsync(ct);
            return createdSession;
        });

        if (session is null)
        {
            AddError(studentAccessCodeClass.StudentAccessCodeErrors.Invalid.Description);

            await Send.ErrorsAsync(
                StatusCodes.Status401Unauthorized,
                ct);

            return;
        }

        cookieService.ClearAuthenticationCookies();
        cookieService.SetAccessTokenCookie(session.AccessToken);

        await Send.NoContentAsync(cancellation: ct);
    }

    private static Task<int?> FindValidCodeOwnerAsync(
        ApplicationDbContext context,
        string codeHash,
        CancellationToken ct
    ) =>
        context.StudentAccessCodes
            .Where(code => code.AccessCodeHash == codeHash && code.ExpiresAt > DateTimeOffset.UtcNow)
            .Select(code => (int?)code.UserId)
            .SingleOrDefaultAsync(ct);
}