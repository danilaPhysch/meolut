namespace EphemerisHub.Models;

public class RinexBeidouEphemeris : RinexEphemeris
{
    // Строка 1: AODE, Crs, Delta n, M0
    public double Aode { get; set; }
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
    
    // Строка 5: IDOT, spare, BDT Week, spare
    public double Idot { get; set; }
    public double Spare1 { get; set; }
    public double BdtWeek { get; set; }
    public double Spare2 { get; set; }
    
    // Строка 6: SV accuracy, SatH1, TGD1, TGD2
    public double SvAccuracy { get; set; }
    public double SatH1 { get; set; }
    public double Tgd1 { get; set; }
    public double Tgd2 { get; set; }
    
    // Строка 7: Transmission time, AODC
    public double TransmissionTime { get; set; }
    public double Aodc { get; set; }
}