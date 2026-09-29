using Attendify.Common.Authentication;
using System.Security.Claims;

namespace Attendify.Features.Settings.GetAttendancePreference;

public sealed class GetAttendancePreferenceEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<GetAttendancePreferenceResponse>
{
    public override void Configure()
    {
        Get("/attendance-preference");
        Group<SettingsGroup>();
        Policies(JwtOptions.AuthenticatedUserPolicy);
        Description(x => x.WithName("GetAttendancePreference"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(AttendifyClaimTypes.UserId), out int userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        bool? enabled = await dbContext.Users
            .Where(user => user.Id == userId && !user.IsDeleted)
            .Select(user => (bool?)user.AttendanceEnabled)
            .SingleOrDefaultAsync(ct);

        if (enabled is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(new GetAttendancePreferenceResponse(enabled.Value), ct);
    }
}
