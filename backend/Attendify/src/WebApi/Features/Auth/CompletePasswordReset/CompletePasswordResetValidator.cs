using Attendify.Common.Domain.Users;

namespace Attendify.Features.Auth.CompletePasswordReset;

public sealed class CompletePasswordResetValidator : Validator<CompletePasswordResetRequest>
{
    private const int SecurityCodeLength = 9;
    private const string SecurityCodePattern = "^[0-9A-HJ-NP-Z]{9}$";

    public CompletePasswordResetValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(User.EmailMaxLength);

        RuleFor(request => request.SecurityCode)
            .NotEmpty()
            .Length(SecurityCodeLength)
            .Matches(SecurityCodePattern);

        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);
    }
}
