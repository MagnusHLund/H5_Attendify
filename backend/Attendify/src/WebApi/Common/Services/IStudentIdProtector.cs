namespace Attendify.Common.Services;

public interface IStudentIdProtector
{
    string Protect(string studentId);
    string Unprotect(string protectedStudentId);
}
