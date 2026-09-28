namespace Attendify.Common.Domain.Authentication;

public static class StudentAccessCodeErrors
{
    public static readonly Error NotFound = Error.NotFound(
       "StudentAccessCode.NotFound",
       "Student access code was not found");

    public static readonly Error InvalidUser = Error.Validation(
        "StudentAccessCode.InvalidUser",
        "The current user is invalid");

    public static readonly Error PersistenceFailed = Error.Failure(
        "StudentAccessCode.PersistenceFailed",
        "The student access code could not be saved");
}