using EphemerisHub.Infrastructure;
using EphemerisHub.Infrastructure.Settings;
using EphemerisHub.Models;

namespace EphemerisHub.Application;

public class TleDownloadService(
    IServiceScopeFactory scopeFactory,
    TleLoaderSettings settings,
    ILogger<TleDownloadService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{ClassName} launched. Interval: {Interval}", nameof(TleDownloadService), settings.ExecuteInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            using var scope = scopeFactory.CreateScope();
            var tleRepository = scope.ServiceProvider.GetRequiredService<ITleRepository>();
            var tleAdapter = scope.ServiceProvider.GetRequiredService<ITleAdapter>();

            try
            {
                var tles = await tleAdapter.GetTles();
                var tlesForSaving = tles.Select(x => new Tle { CsSatNum = x.CsSatNum, Time = now, Line1 = x.Line1, Line2 = x.Line2, Name = x.Name }.MapToEntity()).ToList();
                await tleRepository.SaveTles(tlesForSaving, stoppingToken);
                logger.LogInformation("Loaded TLE: {Count}", tles.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation("{ClassName} stopped.", nameof(TleDownloadService));
            }
            catch (Exception ex) when (
                ex is not OutOfMemoryException &&
                ex is not StackOverflowException &&
                ex is not AccessViolationException)
            {
                logger.LogError(ex, "Error in {ClassName}", nameof(TleDownloadService));
            }

            try
            {
                await Task.Delay(settings.ExecuteInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("{ClassName} stopped.", nameof(TleDownloadService));
    }
}