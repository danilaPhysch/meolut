namespace EphemerisHub.Models;

public class Tle
{
    public required string Name { get; set; }
    public required string Line1 { get; set; }
    public required string Line2 { get; set; }
    public required int CsSatNum { get; set; }
    public required DateTime Time { get; set; }
}