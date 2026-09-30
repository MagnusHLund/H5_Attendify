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

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        DateOnly recordCutoff = DateOnly.FromDateTime(now.UtcDateTime.AddDays(-retention.AttendanceRecordDays));
        await DeleteCategoryAsync(
            "attendance records",
            () => dbContext.AttendanceRecords
                .Where(record => record.AttendanceDate < recordCutoff)
                .ExecuteDeleteAsync(ct),
            ct);

        DateTimeOffset detectionCutoff = now.AddDays(-retention.AttendanceDetectionDays);
        await DeleteCategoryAsync(
            "attendance detections",
            () => dbContext.AttendanceDetections
                .Where(detection => detection.DetectedAt < detectionCutoff)
                .ExecuteDeleteAsync(ct),
            ct);

        DateOnly eventCutoff = DateOnly.FromDateTime(now.UtcDateTime.AddDays(-retention.AttendanceEventDays));
        await DeleteCategoryAsync(
            "attendance events",
            () => dbContext.Attendances
                .Where(attendance => attendance.AttendanceDate < eventCutoff)
                .ExecuteDeleteAsync(ct),
            ct);
    }

    private async Task DeleteCategoryAsync(string category, Func<Task<int>> delete, CancellationToken ct)
    {
        try
        {
            int deleted = await delete();
            _logger.Information(
                "Deleted {Count} {Category} past the configured retention period.", deleted, category);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Retention cleanup failed for {Category}.", category);
        }
    }
}
