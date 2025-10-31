using System.ComponentModel.DataAnnotations.Schema;

namespace TleApi.Models;

/// <summary>
/// Database entity for TLE (Two-Line Element) data
/// </summary>
public class TleEntity
{
    [Column("system")]
    public required string System { get; set; }
    
    [Column("prn")]
    public required string Prn { get; set; }
    
    [Column("epoch")]
    public DateTime Epoch { get; set; }
    
    [Column("line1")]
    public required string Line1 { get; set; }
    
    [Column("line2")]
    public required string Line2 { get; set; }
}
