using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Common.Domain.FacialRecognition;
using Attendify.Common.Persistence;
using System.Security.Claims;

namespace Attendify.Features.Settings.UpdateAttendancePreference;

public sealed class UpdateAttendancePreferenceEndpoint(
    ApplicationDbContext dbContext,
    IAuthenticationCookieService cookieService,
    IServiceScopeFactory scopeFactory
) : Endpoint<UpdateAttendancePreferenceRequest>
{
    public override void Configure()
    {
        Patch("/attendance-preference");
        Group<SettingsGroup>();
        Policies(JwtOptions.StudentPolicy);
        Description(x => x.WithName("UpdateAttendancePreference"));
    }

    public override async Task HandleAsync(UpdateAttendancePreferenceRequest request, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(AttendifyClaimTypes.UserId), out int userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        UpdateResult result = await strategy.ExecuteAsync(async () =>
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            ApplicationDbContext attempt = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await using var transaction = await attempt.Database.BeginTransactionAsync(ct);
            if (!await ActiveUserLock.AcquireAsync(attempt, userId, ct))
                return UpdateResult.Unauthorized;
            User user = await attempt.Users.SingleAsync(candidate => candidate.Id == userId, ct);

            if (request.Enabled == true)
            {
                bool hasFaceProfile = await attempt.FacialProfiles
                    .AnyAsync(profile => profile.UserId == userId && profile.FacialEmbeddings.Any(), ct);
                if (!hasFaceProfile)
                    return UpdateResult.MissingFaceProfile;
            }
            else
            {
                FacialProfile? profile = await attempt.FacialProfiles
                    .Include(candidate => candidate.FacialEmbeddings)
                    .SingleOrDefaultAsync(candidate => candidate.UserId == userId, ct);
                if (profile is not null)
                {
                    attempt.FacialEmbeddings.RemoveRange(profile.FacialEmbeddings);
                    attempt.FacialProfiles.Remove(profile);
                }
            }

            user.AttendanceEnabled = request.Enabled!.Value;
            await attempt.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return UpdateResult.Updated;
        });

        if (result == UpdateResult.Unauthorized)
        {
            cookieService.ClearAuthenticationCookies();
            await Send.UnauthorizedAsync(ct);
            return;
        }
        if (result == UpdateResult.MissingFaceProfile)
        {
            AddError("Add face photos before enabling attendance recognition.");
            await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
            return;
        }
        await Send.NoContentAsync(ct);
    }

    private enum UpdateResult { Updated, Unauthorized, MissingFaceProfile }
}
