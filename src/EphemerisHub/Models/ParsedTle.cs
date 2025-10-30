namespace EphemerisHub.Models;

public record ParsedTle(string Name, string Line1, string Line2, int NoradId)
{
    public int CsSatNum { get; set; }
}