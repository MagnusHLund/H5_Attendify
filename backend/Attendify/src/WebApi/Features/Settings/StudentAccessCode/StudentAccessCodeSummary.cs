namespace Attendify.Features.Settings.StudentAccessCode;

public sealed class StudentAccessCodeSummary : Summary<StudentAccessCodeEndpoint>
{
    public StudentAccessCodeSummary()
    {
        Summary = "Returns a student access code";
        Description = "Generates and responds with a plaintext student access code every new date that is hashed and persisted.";
        Response(200, "Student access code generated");
        Response(400, "The current user is invalid");
        Response(500, "The student access code could not be persisted");
    }
}