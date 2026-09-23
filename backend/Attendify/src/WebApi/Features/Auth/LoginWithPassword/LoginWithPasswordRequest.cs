namespace Attendify.Features.Auth.LoginWithPassword;

public sealed class LoginWithPasswordRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
};
