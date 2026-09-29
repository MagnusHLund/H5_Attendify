namespace Attendify.Features.Attendance.CreateAttendance;

public sealed class CreateAttendanceValidator : Validator<CreateAttendanceRequest>
{
    private const int MaxPhotoSizeInBytes = 5 * 1024 * 1024;

    public CreateAttendanceValidator()
    {
        RuleFor(request => request.Classroom).NotEmpty().MaximumLength(100);

        RuleFor(request => request.Picture).NotNull().WithMessage("A picture is required.");
        RuleFor(request => request.Picture.Length)
            .GreaterThan(0)
            .WithMessage("The picture cannot be empty.")
            .LessThanOrEqualTo(MaxPhotoSizeInBytes)
            .WithMessage("The picture cannot exceed 5 MB.");
        RuleFor(request => request.Picture.ContentType)
            .Equal("image/jpeg")
            .WithMessage("The picture must be a JPEG image.");
    }
}
