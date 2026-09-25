namespace Attendify.Features.Settings.UpdateFacePictures;

public sealed class UpdateFacePicturesValidator : Validator<UpdateFacePicturesRequest>
{
    private const int MaxBase64PhotoLength = 5 * 1024 * 1024 / 3 * 4;

    public UpdateFacePicturesValidator()
    {
        RuleFor(request => request.StraightPhoto)
            .NotEmpty()
            .MaximumLength(MaxBase64PhotoLength);

        RuleFor(request => request.LeftPhoto)
            .NotEmpty()
            .MaximumLength(MaxBase64PhotoLength);

        RuleFor(request => request.RightPhoto)
            .NotEmpty()
            .MaximumLength(MaxBase64PhotoLength);
    }
}
