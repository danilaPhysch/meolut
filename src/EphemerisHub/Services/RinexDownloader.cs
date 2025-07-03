using EphemerisHub.Infrastructure.Configuration;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace EphemerisHub.Services;

public interface IRinexDownloader
{
    Task<string[]> DownloadDailyFilesAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<string> DownloadFileAsync(string fileName, DateTime date, CancellationToken cancellationToken = default);
}

public class RinexDownloader : IRinexDownloader
{
    private readonly HttpClient _httpClient;
    private readonly RinexConfiguration _config;
    private readonly ILogger<RinexDownloader> _logger;

    public RinexDownloader(HttpClient httpClient, RinexConfiguration config, ILogger<RinexDownloader> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<string[]> DownloadDailyFilesAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var downloadedFiles = new List<string>();
        
        // Format: BRDM00DLR_S_20251830000_01D_MN.rnx.gz
        var year = date.Year;
        var dayOfYear = date.DayOfYear;
        var fileName = $"BRDM00DLR_S_{year}{dayOfYear:D3}0000_01D_MN.rnx.gz";
        
        try
        {
            var filePath = await DownloadFileAsync(fileName, date, cancellationToken);
            downloadedFiles.Add(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file {FileName} for date {Date}", fileName, date);
        }
        
        return downloadedFiles.ToArray();
    }

    public async Task<string> DownloadFileAsync(string fileName, DateTime date, CancellationToken cancellationToken = default)
    {
        var year = date.Year;
        var dayOfYear = date.DayOfYear;
        var url = $"{_config.BaseUrl.TrimEnd('/')}/{year}/{dayOfYear:D3}/{fileName}";
        
        var downloadPath = Path.Combine(_config.DownloadDirectory, $"{year}", $"{dayOfYear:D3}");
        Directory.CreateDirectory(downloadPath);
        
        var filePath = Path.Combine(downloadPath, fileName);
        var extractedPath = Path.Combine(downloadPath, Path.GetFileNameWithoutExtension(fileName));
        
        // Check if file already exists
        if (File.Exists(extractedPath))
        {
            _logger.LogInformation("File {FilePath} already exists, skipping download", extractedPath);
            return extractedPath;
        }
        
        _logger.LogInformation("Downloading {Url} to {FilePath}", url, filePath);
        
        var attempt = 0;
        while (attempt < _config.MaxRetryAttempts)
        {
            try
            {
                using var response = await _httpClient.GetAsync(url, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    await using var fileStream = File.Create(filePath);
                    await response.Content.CopyToAsync(fileStream, cancellationToken);
                    
                    _logger.LogInformation("Successfully downloaded {FileName}", fileName);
                    
                    // Extract .gz file
                    await ExtractGzipFileAsync(filePath, extractedPath, cancellationToken);
                    
                    // Remove compressed file after extraction
                    File.Delete(filePath);
                    
                    return extractedPath;
                }
                
                _logger.LogWarning("Failed to download {Url}, status: {StatusCode}", url, response.StatusCode);
                break;
            }
            catch (Exception ex) when (attempt < _config.MaxRetryAttempts - 1)
            {
                attempt++;
                _logger.LogWarning(ex, "Download attempt {Attempt} failed for {Url}, retrying...", attempt, url);
                await Task.Delay(_config.RetryDelay, cancellationToken);
            }
        }
        
        throw new InvalidOperationException($"Failed to download {fileName} after {_config.MaxRetryAttempts} attempts");
    }

    private static async Task ExtractGzipFileAsync(string gzipFilePath, string extractedFilePath, CancellationToken cancellationToken = default)
    {
        await using var gzipStream = new FileStream(gzipFilePath, FileMode.Open, FileAccess.Read);
        await using var decompressionStream = new GZipStream(gzipStream, CompressionMode.Decompress);
        await using var outputStream = new FileStream(extractedFilePath, FileMode.Create, FileAccess.Write);
        
        await decompressionStream.CopyToAsync(outputStream, cancellationToken);
    }
}