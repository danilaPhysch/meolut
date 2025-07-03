namespace EphemerisHub.Infrastructure.Configuration;

public class RinexConfiguration
{
    public const string Section = "Rinex";
    
    public string BaseUrl { get; set; } = "https://cddis.nasa.gov/archive/gnss/data/daily/";
    public string DownloadDirectory { get; set; } = "./downloads";
    public TimeSpan ScheduleInterval { get; set; } = TimeSpan.FromHours(24);
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMinutes(5);
    public bool AutoDownload { get; set; } = true;
    public int DaysToDownload { get; set; } = 1; // How many days back to download
}