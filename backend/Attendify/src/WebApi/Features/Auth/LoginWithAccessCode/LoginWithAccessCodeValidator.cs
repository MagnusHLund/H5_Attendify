namespace Attendify.Features.Auth.LoginWithAccessCode;

public sealed class LoginWithAccessCodeValidator : Validator<LoginWithAccessCodeRequest>
{
    public LoginWithAccessCodeValidator()
    {
        RuleFor(request => request.StudentAccessCode).NotEmpty();
    }
}
