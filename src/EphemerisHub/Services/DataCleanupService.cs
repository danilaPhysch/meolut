using EphemerisHub.Application;
using EphemerisHub.Infrastructure.Settings;

namespace EphemerisHub.Services;

public class DataCleanupService(
    IServiceScopeFactory scopeFactory,
    DataCleanupSettings settings,
    ILogger<DataCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{ClassName} launched. Retention days: {RetentionDays}", 
            nameof(DataCleanupService), settings.RetentionDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, settings.CleanupHour, 0, 0, DateTimeKind.Utc);
                
                if (nextRun <= now)
                {
                    nextRun = nextRun.AddDays(1);
                }

                var delay = nextRun - now;
                logger.LogInformation("Next cleanup scheduled at {NextRun} (in {Delay})", nextRun, delay);

                await Task.Delay(delay, stoppingToken);

                using var scope = scopeFactory.CreateScope();
                var tleRepository = scope.ServiceProvider.GetRequiredService<ITleRepository>();

                var cutoffDate = DateTime.UtcNow.AddDays(-settings.RetentionDays);
                logger.LogInformation("Starting cleanup of TLE data older than {CutoffDate}", cutoffDate);

                await tleRepository.DeleteOldTles(cutoffDate, stoppingToken);
                
                logger.LogInformation("Cleanup completed successfully");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("{ClassName} stopped.", nameof(DataCleanupService));
            }
            catch (Exception ex) when (
                ex is not OutOfMemoryException &&
                ex is not StackOverflowException &&
                ex is not ThreadAbortException &&
                ex is not AccessViolationException)
            {
                logger.LogError(ex, "Error in {ClassName}", nameof(DataCleanupService));
            }
        }

        logger.LogInformation("{ClassName} stopped.", nameof(DataCleanupService));
    }
}
