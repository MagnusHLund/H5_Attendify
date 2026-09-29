namespace Attendify.Common.Services;

/// <summary>
/// Retention periods are set by the responsible controller for each purpose.
/// Null means that automated deletion for that category is not configured.
/// </summary>
public sealed class DataRetentionOptions
{
    public const string SectionName = "DataRetention";

    public int? AttendanceRecordDays { get; init; }
    public int? AttendanceDetectionDays { get; init; }
    public int? AttendanceEventDays { get; init; }
}
