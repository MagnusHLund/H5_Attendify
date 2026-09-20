using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Attendance;
using Attendify.Common.Domain.Authentication;
using Attendify.Common.Domain.FacialRecognition;
using Attendify.Common.Domain.EducationalInstitute;
using EducationalInstituteEntity = Attendify.Common.Domain.EducationalInstitute.EducationalInstitute;

namespace Attendify.Common.Domain.Users;

public sealed class User : AggregateRoot<int>
{
    public const int EmailMaxLength = 320;
    public const int PasswordHashMaxLength = 512;
    public const int EncryptedStudentIdMaxLength = 512;

    public Guid EducationalInstituteId { get; set; }

    public string Email
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Email));
            ThrowIfGreaterThan(value.Length, EmailMaxLength, nameof(Email));
            field = value;
        }
    } = null!;

    public string PasswordHash
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(PasswordHash));
            ThrowIfGreaterThan(value.Length, PasswordHashMaxLength, nameof(PasswordHash));
            field = value;
        }
    } = null!;

    public string EncryptedStudentId
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(EncryptedStudentId));
            ThrowIfGreaterThan(value.Length, EncryptedStudentIdMaxLength, nameof(EncryptedStudentId));
            field = value;
        }
    } = null!;

    public bool AttendanceEnabled { get; set; }

    public EducationalInstituteEntity EducationalInstitute { get; set; } = null!;

    public FacialProfile? FacialProfile { get; set; }

    public ICollection<AttendanceDetection> AttendanceDetections { get; } = [];

    public ICollection<AttendanceRecord> AttendanceRecords { get; } = [];

    public ICollection<AdminAccessCode> AdminAccessCodes { get; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; } = [];

    private User() { }

    public static User Create(
        Guid educationalInstituteId,
        string email,
        string passwordHash,
        string encryptedStudentId
    ) =>
        new()
        {
            EducationalInstituteId = educationalInstituteId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            EncryptedStudentId = encryptedStudentId,
            AttendanceEnabled = true,
        };
}
