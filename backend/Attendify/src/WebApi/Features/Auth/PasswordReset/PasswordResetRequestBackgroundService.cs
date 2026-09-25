namespace Attendify.Features.Auth.PasswordReset;

public sealed class PasswordResetRequestBackgroundService(
    PasswordResetRequestQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<PasswordResetRequestBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (string email in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                IPasswordResetService passwordResetService =
                    scope.ServiceProvider.GetRequiredService<IPasswordResetService>();

                await passwordResetService.RequestPasswordResetAsync(email, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process a password reset request.");
            }
        }
    }
}
