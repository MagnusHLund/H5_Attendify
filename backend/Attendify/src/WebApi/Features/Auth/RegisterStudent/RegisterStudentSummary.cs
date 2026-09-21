namespace Attendify.Features.Auth.RegisterStudent;

public sealed class RegisterStudentSummary : Summary<RegisterStudentEndpoint>
{
    public RegisterStudentSummary()
    {
        Summary = "Registers a student";
        Description = "Creates a student account for an existing educational institute.";
        Response<RegisterStudentResponse>(201, "The newly registered user.");
    }
}
