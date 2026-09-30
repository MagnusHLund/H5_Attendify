using System.Security.Cryptography;
using System.Text;
using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.Users;
using studentAccessCodeClass = Attendify.Common.Domain.Authentication;

namespace Attendify.Features.Settings.StudentAccessCode;

public sealed class StudentAccessCodeGenerator(
    IConfiguration configuration)
    : IStudentAccessCodeGenerator
{
    private const string AllowedCharacters =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private const int CodeLength = 12;

    private readonly byte[] _secret = Encoding.ASCII.GetBytes(
    configuration.GetRequiredSection("Parameters:StudentAccessCodeGenSecret").Value
        ?? throw new InvalidOperationException(
            "Configuration 'Parameters:StudentAccessCodeGenSecret' is missing or empty."));

    public Task<(
        studentAccessCodeClass.StudentAccessCode entity,
        string plainTextCode
    )> GenerateAccessCodeAsync(
        int userId,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var generatedAt = TimeProvider.System.GetUtcNow();

        var generationDate = DateOnly.FromDateTime(
            generatedAt.UtcDateTime);

        var plainTextCode = GenerateCode(
            userId,
            generationDate);

        var codeHash = HashCode(plainTextCode);

        var accessCode =
            studentAccessCodeClass.StudentAccessCode.Create(
                userId: userId,
                code: codeHash,
                generatedAt: generatedAt,
                expiresAt: generatedAt.AddDays(3),
                generationDate: generationDate);

        return Task.FromResult(
            (entity: accessCode, plainTextCode: plainTextCode));
    }

    public string GetPlainTextCode(
        int userId,
        DateOnly generationDate)
    {
        return GenerateCode(userId, generationDate);
    }

    private string GenerateCode(
        int userId,
        DateOnly date)
    {
        using var hmac = new HMACSHA256(_secret);

        var input = Encoding.UTF8.GetBytes(
            $"{userId}:{date:yyyy-MM-dd}");

        var hash = hmac.ComputeHash(input);

        Span<char> code = stackalloc char[CodeLength];

        for (var index = 0; index < code.Length; index++)
        {
            code[index] = AllowedCharacters[
                hash[index] % AllowedCharacters.Length];
        }

        return new string(code);
    }

    public string HashCode(string code)
    {
        var codeBytes = Encoding.UTF8.GetBytes(code);
        var hashBytes = SHA256.HashData(codeBytes);

        return Convert.ToBase64String(hashBytes);
    }
}