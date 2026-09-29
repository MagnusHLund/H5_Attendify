using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.Users;

public interface IStudentAccessCodeGenerator
{
    Task<(StudentAccessCode entity, string plainTextCode)> GenerateAccessCodeAsync(
            int userId,
            CancellationToken ct
    );

    string GetPlainTextCode(
            int userId, DateOnly generationDate
    );

    string HashCode(
            string code
    );
}