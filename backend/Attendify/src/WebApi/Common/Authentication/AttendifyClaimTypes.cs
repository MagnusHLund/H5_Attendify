using System.Security.Claims;

namespace Attendify.Common.Authentication;

public static class AttendifyClaimTypes
{
    public const string UserId = ClaimTypes.NameIdentifier;
    public const string UserType = "user_type";
    public const string StudentId = "student_id";
    public const string TokenFamilyId = "token_family_id";
}
