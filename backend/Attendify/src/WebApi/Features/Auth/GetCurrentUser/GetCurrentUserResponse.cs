using Attendify.Common.Authentication;

namespace Attendify.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserResponse(UserType UserType, string StudentId);
