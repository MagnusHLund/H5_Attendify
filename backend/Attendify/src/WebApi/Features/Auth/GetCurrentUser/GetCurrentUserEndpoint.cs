using Attendify.Common.Authentication;

namespace Attendify.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/me");
        Group<AuthenticationGroup>();
        Policies(JwtOptions.AuthenticatedUserPolicy);
        Description(x => x.WithName("GetCurrentUser"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string? userTypeValue = User.FindFirst(AttendifyClaimTypes.UserType)?.Value;
        string? studentId = User.FindFirst(AttendifyClaimTypes.StudentId)?.Value;

        if (
            !Enum.TryParse<UserType>(userTypeValue, out var userType)
            || string.IsNullOrWhiteSpace(studentId)
        )
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        await Send.OkAsync(new GetCurrentUserResponse(userType, studentId), ct);
    }
}
