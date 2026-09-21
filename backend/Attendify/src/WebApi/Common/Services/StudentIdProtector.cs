using Microsoft.AspNetCore.DataProtection;

namespace Attendify.Common.Services;

public sealed class StudentIdProtector(IDataProtectionProvider dataProtectionProvider)
    : IStudentIdProtector
{
    private const string Purpose = "Attendify.StudentId.v1";

    public string Protect(string studentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(studentId);

        return dataProtectionProvider.CreateProtector(Purpose).Protect(studentId);
    }
}
