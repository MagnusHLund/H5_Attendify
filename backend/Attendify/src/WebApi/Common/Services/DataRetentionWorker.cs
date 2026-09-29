using Attendify.Common.Persistence;
using Microsoft.Extensions.Options;

namespace Attendify.Common.Services;

public sealed class DataRetentionWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<DataRetentionOptions> options
) : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromHours(24);
    private readonly Serilog.ILogger _logger = Log.ForContext<DataRetentionWorker>();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeleteExpiredDataAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Attendance retention cleanup failed.");
            }
            await Task.Delay(SweepInterval, stoppingToken);
        }
    }

    private async Task DeleteExpiredDataAsync(CancellationToken ct)
    {
        DataRetentionOptions retention = options.Value;
        if (retention.AttendanceRecordDays is null && retention.AttendanceDetectionDays is null && retention.AttendanceEventDays is null)
        {
            _logger.Warning("Attendance retention cleanup is not configured; no attendance rows were deleted.");
            return;
        }

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        if (retention.AttendanceRecordDays is int recordDays)
        {
            DateOnly cutoff = DateOnly.FromDateTime(now.UtcDateTime.AddDays(-recordDays));
            int deleted = await dbContext.AttendanceRecords
                .Where(record => record.AttendanceDate < cutoff)
                .ExecuteDeleteAsync(ct);
            _logger.Information("Deleted {Count} attendance records past the configured retention period.", deleted);
        }

        if (retention.AttendanceDetectionDays is int detectionDays)
        {
            DateTimeOffset cutoff = now.AddDays(-detectionDays);
            int deleted = await dbContext.AttendanceDetections
                .Where(detection => detection.DetectedAt < cutoff)
                .ExecuteDeleteAsync(ct);
            _logger.Information("Deleted {Count} attendance detections past the configured retention period.", deleted);
        }

        if (retention.AttendanceEventDays is int eventDays)
        {
            DateOnly cutoff = DateOnly.FromDateTime(now.UtcDateTime.AddDays(-eventDays));
            int deleted = await dbContext.Attendances
                .Where(attendance => attendance.AttendanceDate < cutoff)
                .ExecuteDeleteAsync(ct);
            _logger.Information("Deleted {Count} attendance events past the configured retention period.", deleted);
        }
    }
}
