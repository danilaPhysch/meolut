namespace EphemerisHub.Models;

public class RinexGlonassEphemeris : RinexEphemeris
{
    // Строка 1: pos X, vel X, acc X, health
    public double PosX { get; set; }
    public double VelX { get; set; }
    public double AccX { get; set; }
    public double Health { get; set; }
    
    // Строка 2: pos Y, vel Y, acc Y, frequency number
    public double PosY { get; set; }
    public double VelY { get; set; }
    public double AccY { get; set; }
    public double FreqNum { get; set; }
    
    // Строка 3: pos Z, vel Z, acc Z, age of operation
    public double PosZ { get; set; }
    public double VelZ { get; set; }
    public double AccZ { get; set; }
    public double AgeOfOperation { get; set; }
}