using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.Users;

public interface IStudentAccessCodeGenerator
{
    Task<(StudentAccessCode entity, string plainTextCode)> GenerateAccessCode(
            int userId,
            CancellationToken ct
    );

    string GetPlainTextCode(
            int userId, DateOnly generationDate
    );
}