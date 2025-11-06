namespace EphemerisHub.DTOs;

public class SatelliteDto
{
    public required int CsSatNum { get; set; }
    public required string Name { get; set; }
    public required string System { get; set; }
    public required DateTime LastUpdate { get; set; }
}
