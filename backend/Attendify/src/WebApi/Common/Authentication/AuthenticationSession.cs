namespace Attendify.Common.Authentication;

public sealed record AuthenticationSession(string AccessToken, string RefreshToken);
