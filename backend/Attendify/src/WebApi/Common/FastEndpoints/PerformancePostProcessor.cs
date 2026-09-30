using System.Diagnostics;

namespace Attendify.Common.FastEndpoints;

public class PerformancePostProcessor : IGlobalPostProcessor
{
    private const string ActivityKey = "PerformanceStopwatch";

    private readonly Serilog.ILogger _logger = Log.ForContext<PerformancePostProcessor>();

    public Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        if (
            context.HttpContext.Items.TryGetValue(ActivityKey, out var stopwatchObj)
            && stopwatchObj is Stopwatch stopwatch
        )
        {
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = context.Request?.GetType().Name;
                _logger.Warning(
                    "WebApi Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)",
                    requestName,
                    elapsedMilliseconds
                );
            }
        }

        return Task.CompletedTask;
    }
}
