namespace Attendify.Common.FastEndpoints;

public class LoggingPreProcessor : IGlobalPreProcessor
{
    private readonly Serilog.ILogger _logger = Log.ForContext<LoggingPreProcessor>();

    public Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var requestName = context.Request?.GetType().Name;
        _logger.Information("WebApi Request: {Name}", requestName);

        return Task.CompletedTask;
    }
}
