namespace EphemerisHub.Infrastructure.Settings;

public class DataCleanupSettings
{
    public required int RetentionDays { get; init; }
    public required int CleanupHour { get; init; }
}
