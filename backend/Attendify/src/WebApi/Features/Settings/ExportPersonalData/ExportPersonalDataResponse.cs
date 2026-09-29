namespace Attendify.Features.Settings.ExportPersonalData;

public sealed record ExportPersonalDataResponse(
    DateTimeOffset ExportedAt,
    ExportPersonalDataResponse.AccountData Account,
    IReadOnlyList<ExportPersonalDataResponse.AttendanceEventData> AttendanceEvents,
    IReadOnlyList<ExportPersonalDataResponse.AttendanceData> AttendanceRecords,
    IReadOnlyList<ExportPersonalDataResponse.DetectionData> AttendanceDetections,
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

    public sealed record AttendanceData(
        string Classroom,
        DateOnly Date,
        TimeOnly Arrival,
        TimeOnly Departure,
        bool DepartureKnown
    );

    public sealed record AttendanceEventData(
        string Classroom,
        DateOnly Date,
        TimeOnly ArrivedAt,
        string Status
    );

    public sealed record DetectionData(string Classroom, DateTimeOffset DetectedAt);

    public sealed record AccessCodeData(DateOnly GeneratedOn, DateTimeOffset ExpiresAt);
}
