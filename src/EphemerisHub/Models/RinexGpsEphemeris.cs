namespace EphemerisHub.Models;

public class RinexGpsEphemeris : RinexEphemeris
{
    // Строка 1: IODE, Crs, Delta n, M0
    public double Iode { get; set; }
    public double Crs { get; set; }
    public double DeltaN { get; set; }
    public double M0 { get; set; }
    
    // Строка 2: Cuc, e, Cus, sqrt(A)
    public double Cuc { get; set; }
    public double Eccentricity { get; set; }
    public double Cus { get; set; }
    public double SqrtA { get; set; }
    
    // Строка 3: TOE, Cic, OMEGA0, Cis
    public double Toe { get; set; }
    public double Cic { get; set; }
    public double Omega0 { get; set; }
    public double Cis { get; set; }
    
    // Строка 4: i0, Crc, omega, OMEGA DOT
    public double I0 { get; set; }
    public double Crc { get; set; }
    public double Omega { get; set; }
    public double OmegaDot { get; set; }
    
    // Строка 5: IDOT, Codes on L2, GPS Week, L2 P data flag
    public double Idot { get; set; }
    public double CodesOnL2 { get; set; }
    public double GpsWeek { get; set; }
    public double L2PDataFlag { get; set; }
    
    // Строка 6: SV accuracy, SV health, TGD, IODC
    public double SvAccuracy { get; set; }
    public double SvHealth { get; set; }
    public double Tgd { get; set; }
    public double Iodc { get; set; }
    
    // Строка 7: Transmission time, Fit interval
    public double TransmissionTime { get; set; }
    public double FitInterval { get; set; }
}