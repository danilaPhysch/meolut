namespace EphemerisHub.Infrastructure.Settings;

public class TleSettings
{
    public required HashSet<Uri> Uris { get; init; }
    public int RequestTimeoutSeconds { get; init; } = 30;
}