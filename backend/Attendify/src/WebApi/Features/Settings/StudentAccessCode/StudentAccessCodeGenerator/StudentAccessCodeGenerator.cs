using System.Security.Cryptography;
using System.Text;
using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.Users;
using studentAccessCodeClass = Attendify.Common.Domain.Authentication;

namespace Attendify.Features.Settings.StudentAccessCode;

public sealed class StudentAccessCodeGenerator : IStudentAccessCodeGenerator
{
    private const string AllowedCharacters =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private const int CodeLength = 12;

    public Task<(studentAccessCodeClass.StudentAccessCode entity, string plainTextCode)> GenerateAccessCode(
        int userId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var generatedAt = TimeProvider.System.GetUtcNow();
        var plainTextCode = GenerateCode();
        var codeHash = HashCode(plainTextCode);

        var accessCode = studentAccessCodeClass.StudentAccessCode.Create(
            userId: userId,
            code: codeHash,
            generatedAt: generatedAt,
            expiresAt: generatedAt.AddDays(3),
            generationDate: DateOnly.FromDateTime(
                generatedAt.UtcDateTime));

        return Task.FromResult((accessCode, plainTextCode));
    }

    private static string GenerateCode()
    {
        Span<char> code = stackalloc char[CodeLength];

        for (var index = 0; index < code.Length; index++)
        {
            var characterIndex = RandomNumberGenerator.GetInt32(
                AllowedCharacters.Length);

            code[index] = AllowedCharacters[characterIndex];
        }

        return new string(code);
    }

    private static string HashCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var codeBytes = Encoding.UTF8.GetBytes(code);
        var hashBytes = SHA256.HashData(codeBytes);

        return Convert.ToBase64String(hashBytes);
    }
}