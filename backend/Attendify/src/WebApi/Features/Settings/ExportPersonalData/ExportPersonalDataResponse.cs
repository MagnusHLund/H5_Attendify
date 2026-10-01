namespace Attendify.Features.Settings.ExportPersonalData;

public sealed record ExportPersonalDataResponse(
    DateTimeOffset ExportedAt,
    ExportPersonalDataResponse.AccountData Account,
    IReadOnlyList<ExportPersonalDataResponse.AttendanceEventData> AttendanceEvents,
    IReadOnlyList<ExportPersonalDataResponse.AccessCodeData> StudentAccessCodes
)
{
    public sealed record AccountData(
        string Email,
        string StudentId,
        Guid EducationalInstituteId,
        DateTimeOffset RegisteredAt,
        bool AttendanceEnabled
    );

    public sealed record AttendanceEventData(
        string Classroom,
        DateOnly Date,
        TimeOnly ArrivedAt,
        string Status
    );

    public sealed record AccessCodeData(DateOnly GeneratedOn, DateTimeOffset ExpiresAt);
}
