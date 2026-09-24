namespace Attendify.Features.Auth.RegisterStudent;

public sealed class RegisterStudentSummary : Summary<RegisterStudentEndpoint>
{
    public RegisterStudentSummary()
    {
        Summary = "Registers a student";
        Description =
            "Creates a student account and facial profile for an existing educational institute, then starts an authenticated session.";
        Response(201, "The student account was created and authentication cookies were set.");
        Response(400, "The request, institute selection, or face photos are invalid.");
        Response(409, "An account with the supplied email already exists.");
    }
}
