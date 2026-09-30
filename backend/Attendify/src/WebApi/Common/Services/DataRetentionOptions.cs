namespace Attendify.Common.Services;

/// <summary>
/// Retention periods, in days, approved by the responsible controller for each data category.
/// All periods are required; the API refuses to start without them.
/// </summary>
public sealed class DataRetentionOptions
{
    public const string SectionName = "DataRetention";

    public int AttendanceRecordDays { get; init; }
    public int AttendanceDetectionDays { get; init; }
    public int AttendanceEventDays { get; init; }
}
