namespace Attendify.Features.Settings.UpdateAttendancePreference;

public sealed class UpdateAttendancePreferenceValidator : Validator<UpdateAttendancePreferenceRequest>
{
    public UpdateAttendancePreferenceValidator()
    {
        RuleFor(request => request.Enabled).NotNull();
    }
}
