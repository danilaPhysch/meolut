namespace Meolut.Core.Models;

/// <summary>
/// RINEX file header information
/// </summary>
public class RinexHeader
{
    /// <summary>
    /// RINEX version and file type
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    /// <summary>
    /// File type (N = Navigation data)
    /// </summary>
    public char FileType { get; set; }
    
    /// <summary>
    /// Satellite system (M = Mixed, G = GPS, R = GLONASS, E = Galileo, C = BeiDou)
    /// </summary>
    public char SatelliteSystem { get; set; }
    
    /// <summary>
    /// Program name that created the file
    /// </summary>
    public string ProgramName { get; set; } = string.Empty;
    
    /// <summary>
    /// Run by
    /// </summary>
    public string RunBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of file creation
    /// </summary>
    public DateTime CreationDate { get; set; }
    
    /// <summary>
    /// Time of first observation
    /// </summary>
    public DateTime? TimeOfFirstObs { get; set; }
    
    /// <summary>
    /// Time of last observation
    /// </summary>
    public DateTime? TimeOfLastObs { get; set; }
    
    /// <summary>
    /// Leap seconds
    /// </summary>
    public int? LeapSeconds { get; set; }
    
    /// <summary>
    /// IONOSPHERIC CORR parameters
    /// </summary>
    public Dictionary<string, double[]> IonosphericCorr { get; set; } = new();
    
    /// <summary>
    /// TIME SYSTEM CORR parameters
    /// </summary>
    public Dictionary<string, double[]> TimeSystemCorr { get; set; } = new();
    
    /// <summary>
    /// Comments
    /// </summary>
    public List<string> Comments { get; set; } = new();
}