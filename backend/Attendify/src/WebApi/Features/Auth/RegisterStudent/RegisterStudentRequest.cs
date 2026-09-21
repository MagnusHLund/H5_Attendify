namespace Attendify.Features.Auth.RegisterStudent;

public sealed class RegisterStudentRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public Guid EducationalInstituteId { get; init; }
    public string StudentId { get; init; } = string.Empty;
    public string StraightPhoto { get; init; } = null!;
    public string LeftPhoto { get; init; } = null!;
    public string RightPhoto { get; init; } = null!;
}
