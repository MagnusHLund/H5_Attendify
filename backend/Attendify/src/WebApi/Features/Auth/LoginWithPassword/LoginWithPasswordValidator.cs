using Attendify.Common.Domain.Users;

namespace Attendify.Features.Auth.LoginWithPassword;

public sealed class LoginWithPasswordValidator : Validator<LoginWithPasswordRequest>
{
    public LoginWithPasswordValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(User.EmailMaxLength);

        RuleFor(request => request.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}
