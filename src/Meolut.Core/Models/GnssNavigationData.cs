using System.ComponentModel.DataAnnotations;

namespace Meolut.Core.Models;

/// <summary>
/// Base class for GNSS navigation data
/// </summary>
public abstract class GnssNavigationData
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Satellite PRN (Pseudo Random Number)
    /// </summary>
    public int SatellitePrn { get; set; }
    
    /// <summary>
    /// GNSS system identifier (G=GPS, R=GLONASS, E=Galileo, C=BeiDou)
    /// </summary>
    public char GnssSystem { get; set; }
    
    /// <summary>
    /// Epoch time of clock (Time of Clock)
    /// </summary>
    public DateTime EpochTime { get; set; }
    
    /// <summary>
    /// Clock bias (seconds)
    /// </summary>
    public double ClockBias { get; set; }
    
    /// <summary>
    /// Clock drift (seconds/second)
    /// </summary>
    public double ClockDrift { get; set; }
    
    /// <summary>
    /// Clock drift rate (seconds/second²)
    /// </summary>
    public double ClockDriftRate { get; set; }
    
    /// <summary>
    /// Time when the data was downloaded
    /// </summary>
    public DateTime DownloadTime { get; set; }
}

/// <summary>
/// GPS navigation data
/// </summary>
public class GpsNavigationData : GnssNavigationData
{
    /// <summary>
    /// Issue of Data, Ephemeris
    /// </summary>
    public double Iode { get; set; }
    
    /// <summary>
    /// Radial sine harmonic correction term
    /// </summary>
    public double Crs { get; set; }
    
    /// <summary>
    /// Mean motion difference from computed value
    /// </summary>
    public double DeltaN { get; set; }
    
    /// <summary>
    /// Mean anomaly at reference time
    /// </summary>
    public double M0 { get; set; }
    
    /// <summary>
    /// Amplitude of the cosine harmonic correction term to the argument of latitude
    /// </summary>
    public double Cuc { get; set; }
    
    /// <summary>
    /// Eccentricity
    /// </summary>
    public double Eccentricity { get; set; }
    
    /// <summary>
    /// Amplitude of the sine harmonic correction term to the argument of latitude
    /// </summary>
    public double Cus { get; set; }
    
    /// <summary>
    /// Square root of the semi-major axis
    /// </summary>
    public double SqrtA { get; set; }
    
    /// <summary>
    /// Reference time ephemeris
    /// </summary>
    public double Toe { get; set; }
    
    /// <summary>
    /// Amplitude of the cosine harmonic correction term to the angle of inclination
    /// </summary>
    public double Cic { get; set; }
    
    /// <summary>
    /// Longitude of ascending node of orbit plane at weekly epoch
    /// </summary>
    public double Omega0 { get; set; }
    
    /// <summary>
    /// Amplitude of the sine harmonic correction term to the angle of inclination
    /// </summary>
    public double Cis { get; set; }
    
    /// <summary>
    /// Inclination angle at reference time
    /// </summary>
    public double I0 { get; set; }
    
    /// <summary>
    /// Amplitude of the cosine harmonic correction term to the orbit radius
    /// </summary>
    public double Crc { get; set; }
    
    /// <summary>
    /// Argument of perigee
    /// </summary>
    public double Omega { get; set; }
    
    /// <summary>
    /// Rate of right ascension
    /// </summary>
    public double OmegaDot { get; set; }
    
    /// <summary>
    /// Rate of inclination angle
    /// </summary>
    public double Idot { get; set; }
    
    /// <summary>
    /// Codes on L2 channel
    /// </summary>
    public double CodesOnL2 { get; set; }
    
    /// <summary>
    /// GPS Week number
    /// </summary>
    public double GpsWeek { get; set; }
    
    /// <summary>
    /// L2 P data flag
    /// </summary>
    public double L2PDataFlag { get; set; }
    
    /// <summary>
    /// SV accuracy
    /// </summary>
    public double SvAccuracy { get; set; }
    
    /// <summary>
    /// SV health
    /// </summary>
    public double SvHealth { get; set; }
    
    /// <summary>
    /// Total Group Delay
    /// </summary>
    public double Tgd { get; set; }
    
    /// <summary>
    /// Issue of Data, Clock
    /// </summary>
    public double Iodc { get; set; }
    
    /// <summary>
    /// Transmission time of message
    /// </summary>
    public double TransmissionTime { get; set; }
    
    /// <summary>
    /// Fit interval
    /// </summary>
    public double FitInterval { get; set; }
}

/// <summary>
/// GLONASS navigation data
/// </summary>
public class GlonassNavigationData : GnssNavigationData
{
    /// <summary>
    /// Satellite position X coordinate
    /// </summary>
    public double PositionX { get; set; }
    
    /// <summary>
    /// Satellite velocity X component
    /// </summary>
    public double VelocityX { get; set; }
    
    /// <summary>
    /// Satellite acceleration X component
    /// </summary>
    public double AccelerationX { get; set; }
    
    /// <summary>
    /// Satellite health
    /// </summary>
    public double SatelliteHealth { get; set; }
    
    /// <summary>
    /// Satellite position Y coordinate
    /// </summary>
    public double PositionY { get; set; }
    
    /// <summary>
    /// Satellite velocity Y component
    /// </summary>
    public double VelocityY { get; set; }
    
    /// <summary>
    /// Satellite acceleration Y component
    /// </summary>
    public double AccelerationY { get; set; }
    
    /// <summary>
    /// Frequency number
    /// </summary>
    public double FrequencyNumber { get; set; }
    
    /// <summary>
    /// Satellite position Z coordinate
    /// </summary>
    public double PositionZ { get; set; }
    
    /// <summary>
    /// Satellite velocity Z component
    /// </summary>
    public double VelocityZ { get; set; }
    
    /// <summary>
    /// Satellite acceleration Z component
    /// </summary>
    public double AccelerationZ { get; set; }
    
    /// <summary>
    /// Age of operation information
    /// </summary>
    public double AgeOfOperationInfo { get; set; }
}

/// <summary>
/// Galileo navigation data
/// </summary>
public class GalileoNavigationData : GnssNavigationData
{
    /// <summary>
    /// Issue of Data, Navigation batch
    /// </summary>
    public double Iodnav { get; set; }
    
    /// <summary>
    /// Radial sine harmonic correction term
    /// </summary>
    public double Crs { get; set; }
    
    /// <summary>
    /// Mean motion difference from computed value
    /// </summary>
    public double DeltaN { get; set; }
    
    /// <summary>
    /// Mean anomaly at reference time
    /// </summary>
    public double M0 { get; set; }
    
    /// <summary>
    /// Amplitude of the cosine harmonic correction term to the argument of latitude
    /// </summary>
    public double Cuc { get; set; }
    
    /// <summary>
    /// Eccentricity
    /// </summary>
    public double Eccentricity { get; set; }
    
    /// <summary>
    /// Amplitude of the sine harmonic correction term to the argument of latitude
    /// </summary>
    public double Cus { get; set; }
    
    /// <summary>
    /// Square root of the semi-major axis
    /// </summary>
    public double SqrtA { get; set; }
    
    /// <summary>
    /// Reference time ephemeris
    /// </summary>
    public double Toe { get; set; }
    
    /// <summary>
    /// Amplitude of the cosine harmonic correction term to the angle of inclination
    /// </summary>
    public double Cic { get; set; }
    
    /// <summary>
    /// Longitude of ascending node of orbit plane at weekly epoch
    /// </summary>
    public double Omega0 { get; set; }
    
    /// <summary>
    /// Amplitude of the sine harmonic correction term to the angle of inclination
    /// </summary>
    public double Cis { get; set; }
    
    /// <summary>
    /// Inclination angle at reference time
    /// </summary>
    public double I0 { get; set; }
    
    /// <summary>
    /// Amplitude of the cosine harmonic correction term to the orbit radius
    /// </summary>
    public double Crc { get; set; }
    
    /// <summary>
    /// Argument of perigee
    /// </summary>
    public double Omega { get; set; }
    
    /// <summary>
    /// Rate of right ascension
    /// </summary>
    public double OmegaDot { get; set; }
    
    /// <summary>
    /// Rate of inclination angle
    /// </summary>
    public double Idot { get; set; }
    
    /// <summary>
    /// Data sources
    /// </summary>
    public double DataSources { get; set; }
    
    /// <summary>
    /// Galileo Week number
    /// </summary>
    public double GalileoWeek { get; set; }
    
    /// <summary>
    /// SISA Signal in Space Accuracy
    /// </summary>
    public double Sisa { get; set; }
    
    /// <summary>
    /// SV health
    /// </summary>
    public double SvHealth { get; set; }
    
    /// <summary>
    /// BGD E5a/E1 group delay
    /// </summary>
    public double BgdE5aE1 { get; set; }
    
    /// <summary>
    /// BGD E5b/E1 group delay
    /// </summary>
    public double BgdE5bE1 { get; set; }
    
    /// <summary>
    /// Transmission time of message
    /// </summary>
    public double TransmissionTime { get; set; }
}

/// <summary>
/// BeiDou navigation data
/// </summary>
public class BeidouNavigationData : GnssNavigationData
{
    /// <summary>
    /// Age of Data Ephemeris
    /// </summary>
    public double Aode { get; set; }
    
    /// <summary>
    /// Radial sine harmonic correction term
    /// </summary>
    public double Crs { get; set; }
    
    /// <summary>
    /// Mean motion difference from computed value
    /// </summary>
    public double DeltaN { get; set; }
    
    /// <summary>
    /// Mean anomaly at reference time
    /// </summary>
    public double M0 { get; set; }
    
    /// <summary>
    /// Amplitude of the cosine harmonic correction term to the argument of latitude
    /// </summary>
    public double Cuc { get; set; }
    
    /// <summary>
    /// Eccentricity
    /// </summary>
    public double Eccentricity { get; set; }
    
    /// <summary>
    /// Amplitude of the sine harmonic correction term to the argument of latitude
    /// </summary>
    public double Cus { get; set; }
    
    /// <summary>
    /// Square root of the semi-major axis
    /// </summary>
    public double SqrtA { get; set; }
    
    /// <summary>
    /// Reference time ephemeris
    /// </summary>
    public double Toe { get; set; }
    
    /// <summary>
    /// Amplitude of the cosine harmonic correction term to the angle of inclination
    /// </summary>
    public double Cic { get; set; }
    
    /// <summary>
    /// Longitude of ascending node of orbit plane at weekly epoch
    /// </summary>
    public double Omega0 { get; set; }
    
    /// <summary>
    /// Amplitude of the sine harmonic correction term to the angle of inclination
    /// </summary>
    public double Cis { get; set; }
    
    /// <summary>
    /// Inclination angle at reference time
    /// </summary>
    public double I0 { get; set; }
    
    /// <summary>
    /// Amplitude of the cosine harmonic correction term to the orbit radius
    /// </summary>
    public double Crc { get; set; }
    
    /// <summary>
    /// Argument of perigee
    /// </summary>
    public double Omega { get; set; }
    
    /// <summary>
    /// Rate of right ascension
    /// </summary>
    public double OmegaDot { get; set; }
    
    /// <summary>
    /// Rate of inclination angle
    /// </summary>
    public double Idot { get; set; }
    
    /// <summary>
    /// Codes on L2 channel
    /// </summary>
    public double CodesOnL2 { get; set; }
    
    /// <summary>
    /// BeiDou Week number
    /// </summary>
    public double BeidouWeek { get; set; }
    
    /// <summary>
    /// L2 P data flag
    /// </summary>
    public double L2PDataFlag { get; set; }
    
    /// <summary>
    /// SV accuracy
    /// </summary>
    public double SvAccuracy { get; set; }
    
    /// <summary>
    /// SV health
    /// </summary>
    public double SvHealth { get; set; }
    
    /// <summary>
    /// Total Group Delay 1
    /// </summary>
    public double Tgd1 { get; set; }
    
    /// <summary>
    /// Total Group Delay 2
    /// </summary>
    public double Tgd2 { get; set; }
    
    /// <summary>
    /// Transmission time of message
    /// </summary>
    public double TransmissionTime { get; set; }
    
    /// <summary>
    /// Age of Data Clock
    /// </summary>
    public double Aodc { get; set; }
}