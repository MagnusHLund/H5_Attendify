using System.Security.Claims;
using Attendify.Common.Authentication;
using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.Users;

namespace Attendify.Features.Settings.StudentAccessCode;

public class StudentAccessCodeEndpoint(
    ApplicationDbContext dbContext,
    IStudentAccessCodeGenerator accessCodeGenerator,
    IServiceScopeFactory scopeFactory
) : EndpointWithoutRequest<StudentAccessCodeResponse>
{
    public override void Configure()
    {
        Get("/student-access-code");
        Group<SettingsGroup>();
        Policies(JwtOptions.AuthenticatedUserPolicy);
        Description(x => x.WithName("StudentAccessCode"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var userIdValue = User.FindFirstValue(AttendifyClaimTypes.UserId);

            if (!int.TryParse(userIdValue, out var parsedUserId))
            {
                AddError(StudentAccessCodeErrors.InvalidUser.Description);
                await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            UserId userIdClaim = UserId.From(parsedUserId);
            var generationDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var strategy = dbContext.Database.CreateExecutionStrategy();
            string? plainTextCode = await strategy.ExecuteAsync(async () =>
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                ApplicationDbContext attempt =
                    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await using var transaction = await attempt.Database.BeginTransactionAsync(ct);

                if (!await ActiveUserLock.AcquireAsync(attempt, parsedUserId, ct))
                    return null;

                var accessCode = await attempt.StudentAccessCodes.SingleOrDefaultAsync(
                    x => x.UserId == userIdClaim.Value && x.GenerationDate == generationDate,
                    ct
                );

                if (accessCode is not null)
                {
                    await transaction.CommitAsync(ct);
                    return accessCodeGenerator.GetPlainTextCode(
                        userIdClaim.Value,
                        accessCode.GenerationDate
                    );
                }

                var (accessCodeEntity, generatedCode) =
                    await accessCodeGenerator.GenerateAccessCode(userIdClaim.Value, ct);

                attempt.StudentAccessCodes.Add(accessCodeEntity);
                await attempt.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return generatedCode;
            });

            if (plainTextCode is null)
            {
                await Send.UnauthorizedAsync(ct);
                return;
            }

            await Send.OkAsync(new StudentAccessCodeResponse(plainTextCode), cancellation: ct);
        }
        catch (DbUpdateException)
        {
            AddError(StudentAccessCodeErrors.PersistenceFailed.Description);

            await Send.ErrorsAsync(StatusCodes.Status500InternalServerError, ct);
        }
    }
}
