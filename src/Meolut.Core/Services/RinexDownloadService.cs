using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace Meolut.Core.Services;

/// <summary>
/// Service for downloading RINEX files from NASA CDDIS archive
/// </summary>
public class RinexDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RinexDownloadService> _logger;
    private const string CDDIS_BASE_URL = "https://cddis.nasa.gov/archive/gnss/data/daily";

    public RinexDownloadService(HttpClient httpClient, ILogger<RinexDownloadService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Download RINEX navigation file for a specific date
    /// </summary>
    /// <param name="date">Date for which to download the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stream containing the uncompressed RINEX file content</returns>
    public async Task<Stream> DownloadNavigationFileAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var fileName = GenerateFileName(date);
        var url = BuildDownloadUrl(date, fileName);
        
        _logger.LogInformation("Downloading RINEX file from {Url}", url);
        
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var compressedStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            
            // Decompress the .gz file
            var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            var decompressedData = new MemoryStream();
            
            await gzipStream.CopyToAsync(decompressedData, cancellationToken);
            decompressedData.Position = 0;
            
            _logger.LogInformation("Successfully downloaded and decompressed RINEX file: {FileName}", fileName);
            
            return decompressedData;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to download RINEX file from {Url}", url);
            throw new InvalidOperationException($"Failed to download RINEX file from {url}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RINEX file download");
            throw;
        }
    }

    /// <summary>
    /// Check if a RINEX file exists for the given date
    /// </summary>
    /// <param name="date">Date to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the file exists, false otherwise</returns>
    public async Task<bool> FileExistsAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var fileName = GenerateFileName(date);
        var url = BuildDownloadUrl(date, fileName);
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking if RINEX file exists at {Url}", url);
            return false;
        }
    }

    /// <summary>
    /// Get available RINEX files for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available dates</returns>
    public async Task<List<DateOnly>> GetAvailableFilesAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var availableDates = new List<DateOnly>();
        var currentDate = startDate;
        
        while (currentDate <= endDate)
        {
            if (await FileExistsAsync(currentDate, cancellationToken))
            {
                availableDates.Add(currentDate);
            }
            
            currentDate = currentDate.AddDays(1);
        }
        
        _logger.LogInformation("Found {Count} available RINEX files between {StartDate} and {EndDate}", 
            availableDates.Count, startDate, endDate);
        
        return availableDates;
    }

    /// <summary>
    /// Generate RINEX file name for a given date
    /// Format: BRDM00DLR_S_YYYYDDD0000_01D_MN.rnx.gz
    /// Where DDD is the day of year
    /// </summary>
    private string GenerateFileName(DateOnly date)
    {
        var dayOfYear = date.DayOfYear;
        var year = date.Year;
        
        return $"BRDM00DLR_S_{year}{dayOfYear:D3}0000_01D_MN.rnx.gz";
    }

    /// <summary>
    /// Build the full download URL for a RINEX file
    /// </summary>
    private string BuildDownloadUrl(DateOnly date, string fileName)
    {
        var year = date.Year;
        var dayOfYear = date.DayOfYear;
        
        // URL structure: /year/day_of_year/navigation_file
        return $"{CDDIS_BASE_URL}/{year}/{dayOfYear:D3}/{fileName}";
    }
}

/// <summary>
/// Configuration options for RINEX download service
/// </summary>
public class RinexDownloadOptions
{
    /// <summary>
    /// Base URL for CDDIS archive (default: https://cddis.nasa.gov/archive/gnss/data/daily)
    /// </summary>
    public string BaseUrl { get; set; } = "https://cddis.nasa.gov/archive/gnss/data/daily";
    
    /// <summary>
    /// HTTP client timeout in seconds (default: 120)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;
    
    /// <summary>
    /// Maximum number of retry attempts (default: 3)
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Delay between retry attempts in milliseconds (default: 1000)
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;
}