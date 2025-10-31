namespace TleApi.DTOs;

/// <summary>
/// Data transfer object for TLE response
/// </summary>
public class TleDto
{
    public required string System { get; set; }
    public required string Prn { get; set; }
    public required string Epoch { get; set; }
    public required string Line1 { get; set; }
    public required string Line2 { get; set; }
}
