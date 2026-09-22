using Attendify.Common.Domain.Users;

namespace Attendify.Common.Authentication;

public sealed record RotatedRefreshTokenResult(string Token, User User);
