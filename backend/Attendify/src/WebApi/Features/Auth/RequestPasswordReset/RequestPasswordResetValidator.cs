using Attendify.Common.Domain.Users;

namespace Attendify.Features.Auth.RequestPasswordReset;

public sealed class RequestPasswordResetValidator : Validator<RequestPasswordResetRequest>
{
    public RequestPasswordResetValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(User.EmailMaxLength);
    }
}
