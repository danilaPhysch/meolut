namespace EphemerisHub.DTOs;

public class StatusDto
{
    public required string Status { get; set; }
    public required DateTime? LastDownload { get; set; }
    public required Dictionary<string, SystemStatusDto> Systems { get; set; }
}

public class SystemStatusDto
{
    public required int RecordCount { get; set; }
    public required DateTime? LastUpdate { get; set; }
}
