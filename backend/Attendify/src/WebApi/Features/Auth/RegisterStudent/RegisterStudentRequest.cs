namespace Attendify.Features.Auth.RegisterStudent;

public sealed record RegisterStudentRequest(
    string Email,
    string Password,
    Guid EducationalInstituteId,
    string StudentId,
    IFormFile StraightPhoto,
    IFormFile LeftPhoto,
    IFormFile RightPhoto
);
