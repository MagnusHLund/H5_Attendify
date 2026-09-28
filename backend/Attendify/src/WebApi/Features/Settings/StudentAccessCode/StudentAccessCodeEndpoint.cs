using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.Users;
using System.Security.Claims;
using Attendify.Common.Authentication;
using studentAccessCodeClass = Attendify.Common.Domain.Authentication.StudentAccessCode;

namespace Attendify.Features.Settings.StudentAccessCode;

public class StudentAccessCodeEndpoint(
        ApplicationDbContext dbContext,
        IStudentAccessCodeGenerator accessCodeGenerator
) : EndpointWithoutRequest<StudentAccessCodeResponse>
{
    public override void Configure()
    {
        Get("/student-access-code");
        Group<SettingsGroup>();
        Description(x => x.WithName("StudentAccessCode"));
    }

    public override async Task HandleAsync(
        CancellationToken ct
    )
    {
        try
        {
            UserId userIdClaim = UserId.From(int.Parse(User.FindFirstValue(AttendifyClaimTypes.UserId)!));
            var generationDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var accessCode = await dbContext.StudentAccessCodes
                .SingleOrDefaultAsync(
                    x => x.UserId == userIdClaim.Value &&
                    x.GenerationDate == generationDate,
                    ct);

            if (accessCode is null)
            {
                var (accessCodeEntity, plainTextCode) =
                    await accessCodeGenerator.GenerateAccessCode(
                        userIdClaim.Value,
                        ct);

                dbContext.StudentAccessCodes.Add(accessCodeEntity);
                await dbContext.SaveChangesAsync(ct);

                await Send.OkAsync(
                    new StudentAccessCodeResponse(plainTextCode),
                    cancellation: ct);

                return;
            }

            string existingPlainTextCode =
                accessCodeGenerator.GetPlainTextCode(
                    userIdClaim.Value,
                    accessCode.GenerationDate);

            await Send.OkAsync(
                new StudentAccessCodeResponse(existingPlainTextCode),
                cancellation: ct);
        }
        catch (ArgumentException)
        {
            AddError(StudentAccessCodeErrors.InvalidUser.Description);

            await Send.ErrorsAsync(
                StatusCodes.Status400BadRequest,
                ct);
        }
        catch (DbUpdateException)
        {
            AddError(StudentAccessCodeErrors.PersistenceFailed.Description);

            await Send.ErrorsAsync(
                StatusCodes.Status500InternalServerError,
                ct);
        }
    }
}