namespace Attendify.Features.Attendance.CreateAttendance;

public sealed class CreateAttendanceValidator
    : Validator<CreateAttendanceRequest>
{
    private const int MaxPhotoSizeInBytes = 5 * 1024 * 1024;

    private const int MaxBase64Length = ((MaxPhotoSizeInBytes + 2) / 3) * 4;

    public CreateAttendanceValidator()
    {
        RuleFor(request => request.Picture)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(MaxBase64Length)
            .Must(BeValidBase64)
            .WithMessage("Picture must be a valid Base64-encoded image.");

        RuleFor(request => request.Classroom)
            .NotEmpty()
            .MaximumLength(100);
    }

    private static bool BeValidBase64(string picture)
    {
        if (string.IsNullOrWhiteSpace(picture))
        {
            return false;
        }

        try
        {
            byte[] decodedPhoto = Convert.FromBase64String(picture);

            return decodedPhoto.Length > 0 &&
                   decodedPhoto.Length <= MaxPhotoSizeInBytes;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}