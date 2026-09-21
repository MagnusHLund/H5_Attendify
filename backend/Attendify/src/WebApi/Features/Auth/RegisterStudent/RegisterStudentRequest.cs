namespace Attendify.Features.Auth.RegisterStudent;

public sealed class RegisterStudentRequest
{
    [FromForm]
    public string Email { get; init; } = string.Empty;

    [FromForm]
    public string Password { get; init; } = string.Empty;

    [FromForm]
    public Guid EducationalInstituteId { get; init; }

    [FromForm]
    public string StudentId { get; init; } = string.Empty;

    [FromForm]
    public IFormFile StraightPhoto { get; init; } = null!;

    [FromForm]
    public IFormFile LeftPhoto { get; init; } = null!;

    [FromForm]
    public IFormFile RightPhoto { get; init; } = null!;
}
