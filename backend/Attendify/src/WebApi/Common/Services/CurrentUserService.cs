using System.Security.Claims;
using Attendify.Common.Authentication;
using Attendify.Common.Interfaces;

namespace Attendify.Common.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(AttendifyClaimTypes.UserId);

    public UserType? UserType
    {
        get
        {
            var value = User?.FindFirstValue(AttendifyClaimTypes.UserType);

            return Enum.TryParse<UserType>(value, ignoreCase: true, out var userType)
                ? userType
                : null;
        }
    }

    public string? StudentId => User?.FindFirstValue(AttendifyClaimTypes.StudentId);
}
