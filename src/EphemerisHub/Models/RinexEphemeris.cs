namespace EphemerisHub.Models;

public abstract class RinexEphemeris
{
    public string SatelliteSystem { get; set; } // G, R, E, C
    public int SatellitePrn { get; set; }
    public DateTime TimeOfClock { get; set; }

    // Коэффициенты часов спутника
    public double ClockBias { get; set; }
    public double ClockDrift { get; set; }
    public double ClockDriftRate { get; set; }
}