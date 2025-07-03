using EphemerisHub.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EphemerisHub.Services;

public class RinexSchedulerService : BackgroundService
{
    private readonly IRinexDownloader _downloader;
    private readonly IRinexParser _parser;
    private readonly RinexConfiguration _config;
    private readonly ILogger<RinexSchedulerService> _logger;

    public RinexSchedulerService(
        IRinexDownloader downloader,
        IRinexParser parser,
        RinexConfiguration config,
        ILogger<RinexSchedulerService> logger)
    {
        _downloader = downloader;
        _parser = parser;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.AutoDownload)
        {
            _logger.LogInformation("Auto-download is disabled");
            return;
        }

        _logger.LogInformation("RINEX Scheduler Service started with interval: {Interval}", _config.ScheduleInterval);

        // Run initial download
        await ProcessRinexFiles(stoppingToken);

        // Schedule periodic downloads
        using var timer = new PeriodicTimer(_config.ScheduleInterval);
        
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessRinexFiles(stoppingToken);
        }
    }

    private async Task ProcessRinexFiles(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting RINEX file processing...");

            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-_config.DaysToDownload + 1);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await ProcessDateAsync(date, cancellationToken);
            }

            _logger.LogInformation("RINEX file processing completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RINEX file processing");
        }
    }

    private async Task ProcessDateAsync(DateTime date, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing RINEX files for date: {Date:yyyy-MM-dd}", date);

            var downloadedFiles = await _downloader.DownloadDailyFilesAsync(date, cancellationToken);

            foreach (var filePath in downloadedFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    await _parser.ParseAndSaveAsync(filePath, cancellationToken);
                    _logger.LogInformation("Successfully processed file: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse file: {FilePath}", filePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process RINEX files for date: {Date:yyyy-MM-dd}", date);
        }
    }
}