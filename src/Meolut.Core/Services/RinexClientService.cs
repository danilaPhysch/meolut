using Microsoft.Extensions.Logging;
using Meolut.Core.Parsing;

namespace Meolut.Core.Services;

/// <summary>
/// Main service that orchestrates RINEX file download, parsing, and storage
/// </summary>
public class RinexClientService
{
    private readonly RinexDownloadService _downloadService;
    private readonly RinexParser _parser;
    private readonly RinexDataService _dataService;
    private readonly ILogger<RinexClientService> _logger;

    public RinexClientService(
        RinexDownloadService downloadService,
        RinexParser parser,
        RinexDataService dataService,
        ILogger<RinexClientService> logger)
    {
        _downloadService = downloadService;
        _parser = parser;
        _dataService = dataService;
        _logger = logger;
    }

    /// <summary>
    /// Process RINEX file for a specific date
    /// </summary>
    /// <param name="date">Date to process</param>
    /// <param name="forceUpdate">Force update even if data already exists</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processing result</returns>
    public async Task<RinexProcessingResult> ProcessDateAsync(DateOnly date, bool forceUpdate = false, 
        CancellationToken cancellationToken = default)
    {
        var result = new RinexProcessingResult { Date = date };
        
        try
        {
            _logger.LogInformation("Starting RINEX processing for date {Date}", date);

            // Check if data already exists and skip if not forcing update
            if (!forceUpdate && await _dataService.HasDataForDateAsync(date, cancellationToken))
            {
                _logger.LogInformation("Data already exists for date {Date}, skipping", date);
                result.Status = ProcessingStatus.Skipped;
                result.Message = "Data already exists";
                return result;
            }

            // Check if file exists
            if (!await _downloadService.FileExistsAsync(date, cancellationToken))
            {
                _logger.LogWarning("RINEX file not available for date {Date}", date);
                result.Status = ProcessingStatus.FileNotAvailable;
                result.Message = "RINEX file not available";
                return result;
            }

            // Download the file
            _logger.LogInformation("Downloading RINEX file for date {Date}", date);
            using var fileStream = await _downloadService.DownloadNavigationFileAsync(date, cancellationToken);
            result.FileDownloaded = true;

            // Parse the file
            _logger.LogInformation("Parsing RINEX file for date {Date}", date);
            var parseResult = await _parser.ParseAsync(fileStream);
            result.ParseResult = parseResult;

            // Validate parsed data
            var totalRecords = parseResult.GpsData.Count + parseResult.GlonassData.Count + 
                             parseResult.GalileoData.Count + parseResult.BeidouData.Count;
            
            if (totalRecords == 0)
            {
                _logger.LogWarning("No valid navigation data found in RINEX file for date {Date}", date);
                result.Status = ProcessingStatus.NoData;
                result.Message = "No valid navigation data found";
                return result;
            }

            // Save to database
            _logger.LogInformation("Saving {TotalRecords} navigation records to database for date {Date}", 
                totalRecords, date);
            
            var savedRecords = await _dataService.SaveRinexDataAsync(parseResult, cancellationToken);
            result.RecordsSaved = savedRecords;

            result.Status = ProcessingStatus.Success;
            result.Message = $"Successfully processed {savedRecords} records";
            
            _logger.LogInformation("Successfully completed RINEX processing for date {Date}. Saved {SavedRecords} records", 
                date, savedRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RINEX file for date {Date}", date);
            result.Status = ProcessingStatus.Error;
            result.Message = ex.Message;
            result.Exception = ex;
        }

        return result;
    }

    /// <summary>
    /// Process RINEX files for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="forceUpdate">Force update even if data already exists</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of processing results</returns>
    public async Task<List<RinexProcessingResult>> ProcessDateRangeAsync(DateOnly startDate, DateOnly endDate, 
        bool forceUpdate = false, CancellationToken cancellationToken = default)
    {
        var results = new List<RinexProcessingResult>();
        var currentDate = startDate;

        _logger.LogInformation("Starting RINEX processing for date range {StartDate} to {EndDate}", startDate, endDate);

        while (currentDate <= endDate && !cancellationToken.IsCancellationRequested)
        {
            var result = await ProcessDateAsync(currentDate, forceUpdate, cancellationToken);
            results.Add(result);
            
            currentDate = currentDate.AddDays(1);
            
            // Small delay to avoid overwhelming the server
            await Task.Delay(100, cancellationToken);
        }

        var successCount = results.Count(r => r.Status == ProcessingStatus.Success);
        var totalRecords = results.Sum(r => r.RecordsSaved);
        
        _logger.LogInformation("Completed RINEX processing for date range. {SuccessCount}/{TotalCount} dates processed successfully. Total records saved: {TotalRecords}",
            successCount, results.Count, totalRecords);

        return results;
    }

    /// <summary>
    /// Get current data status for a date range
    /// </summary>
    public async Task<List<DataStatusInfo>> GetDataStatusAsync(DateOnly startDate, DateOnly endDate, 
        CancellationToken cancellationToken = default)
    {
        var status = new List<DataStatusInfo>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var hasData = await _dataService.HasDataForDateAsync(currentDate, cancellationToken);
            var fileExists = await _downloadService.FileExistsAsync(currentDate, cancellationToken);

            status.Add(new DataStatusInfo
            {
                Date = currentDate,
                HasData = hasData,
                FileAvailable = fileExists
            });

            currentDate = currentDate.AddDays(1);
        }

        return status;
    }

    /// <summary>
    /// Clean up old data
    /// </summary>
    public async Task<int> CleanupOldDataAsync(int daysToKeep, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        
        _logger.LogInformation("Cleaning up navigation data older than {CutoffDate} ({DaysToKeep} days)", 
            cutoffDate, daysToKeep);
        
        var deletedCount = await _dataService.DeleteOldDataAsync(cutoffDate, cancellationToken);
        
        _logger.LogInformation("Cleanup completed. Deleted {DeletedCount} old records", deletedCount);
        
        return deletedCount;
    }
}

/// <summary>
/// Result of RINEX file processing
/// </summary>
public class RinexProcessingResult
{
    public DateOnly Date { get; set; }
    public ProcessingStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool FileDownloaded { get; set; }
    public RinexParseResult? ParseResult { get; set; }
    public int RecordsSaved { get; set; }
    public Exception? Exception { get; set; }
}

/// <summary>
/// Processing status enumeration
/// </summary>
public enum ProcessingStatus
{
    Success,
    Skipped,
    FileNotAvailable,
    NoData,
    Error
}

/// <summary>
/// Data status information for a specific date
/// </summary>
public class DataStatusInfo
{
    public DateOnly Date { get; set; }
    public bool HasData { get; set; }
    public bool FileAvailable { get; set; }
}