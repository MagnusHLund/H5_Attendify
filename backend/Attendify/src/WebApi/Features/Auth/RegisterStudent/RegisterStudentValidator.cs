using Attendify.Common.Domain.Users;

namespace Attendify.Features.Auth.RegisterStudent;

public sealed class RegisterStudentValidator : Validator<RegisterStudentRequest>
{
    public RegisterStudentValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(User.EmailMaxLength);

        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(request => request.EducationalInstituteId)
            .NotEmpty();

        RuleFor(request => request.StudentId)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(request => request.StraightPhoto)
            .NotEmpty();

        RuleFor(request => request.LeftPhoto)
            .NotEmpty();

        RuleFor(request => request.RightPhoto)
            .NotEmpty();
    }
}
