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
        UserId userIdClaim = UserId.From(int.Parse(User.FindFirstValue(AttendifyClaimTypes.UserId)!));

        var accessCode = await dbContext.StudentAccessCodes
            .SingleOrDefaultAsync(
                x => x.UserId == userIdClaim,
                ct);

        if (accessCode is null)
        {
            (studentAccessCodeClass? accessCodeEntity, string generatedPlainTextCode) = await accessCodeGenerator.GenerateAccessCode(
                userIdClaim.Value,
                ct);

            dbContext.StudentAccessCodes.Add(accessCodeEntity);
            await dbContext.SaveChangesAsync(ct);

            await Send.OkAsync(
                new StudentAccessCodeResponse(generatedPlainTextCode),
                cancellation: ct
            );

        }
    }
}