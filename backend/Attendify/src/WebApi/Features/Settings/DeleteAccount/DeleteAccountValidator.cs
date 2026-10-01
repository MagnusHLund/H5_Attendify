namespace Attendify.Features.Settings.DeleteAccount;

public sealed class DeleteAccountValidator : Validator<DeleteAccountRequest>
{
    public DeleteAccountValidator()
    {
        RuleFor(request => request.CurrentPassword).NotEmpty();
    }
}
