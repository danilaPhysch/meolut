namespace TleApi.Configuration;

/// <summary>
/// Configuration options for TLE settings
/// </summary>
public class TleOptions
{
    public const string SectionName = "Tle";
    
    /// <summary>
    /// Name of the database table containing TLE data (default: "tle")
    /// </summary>
    public string TableName { get; set; } = "tle";
}
