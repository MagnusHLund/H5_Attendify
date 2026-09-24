using Attendify.Common.Domain.Users;

namespace Attendify.Features.Auth.VerifyPasswordReset;

public sealed class VerifyPasswordResetValidator : Validator<VerifyPasswordResetRequest>
{
    private const int SecurityCodeLength = 9;
    private const string SecurityCodePattern = "^[0-9A-HJ-NP-Z]{9}$";

    public VerifyPasswordResetValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(User.EmailMaxLength);

        RuleFor(request => request.SecurityCode)
            .NotEmpty()
            .Length(SecurityCodeLength)
            .Matches(SecurityCodePattern);
    }
}
