namespace Attendify.Features.Auth.LoginWithAccessCode;

public sealed record LoginWithAccessCodeRequest(
        string StudentAccessCode
);