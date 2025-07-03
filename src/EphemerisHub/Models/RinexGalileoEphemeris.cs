namespace EphemerisHub.Models;

public class RinexGalileoEphemeris : RinexEphemeris
{
    // Строка 1: IODnav, Crs, Delta n, M0
    public double Iodnav { get; set; }
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
    
    // Строка 5: IDOT, Data sources, GAL Week, spare
    public double Idot { get; set; }
    public double DataSources { get; set; }
    public double GalWeek { get; set; }
    public double Spare { get; set; }
    
    // Строка 6: SISA, SV health, BGD E5a/E1, BGD E5b/E1
    public double Sisa { get; set; }
    public double SvHealth { get; set; }
    public double BgdE5aE1 { get; set; }
    public double BgdE5bE1 { get; set; }
    
    // Строка 7: Transmission time
    public double TransmissionTime { get; set; }
}