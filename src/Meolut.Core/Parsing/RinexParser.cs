using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Meolut.Core.Models;

namespace Meolut.Core.Parsing;

public class RinexParser
{
    private readonly ILogger<RinexParser> _logger;
    private const double SPEED_OF_LIGHT = 2.99792458e8; // m/s

    public RinexParser(ILogger<RinexParser> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parse RINEX navigation file
    /// </summary>
    public async Task<RinexParseResult> ParseAsync(Stream stream)
    {
        var result = new RinexParseResult();
        
        try
        {
            using var reader = new StreamReader(stream);
            
            // Parse header
            result.Header = await ParseHeaderAsync(reader);
            _logger.LogInformation("Parsed RINEX header: Version {Version}, System {System}", 
                result.Header.Version, result.Header.SatelliteSystem);
            
            // Parse navigation data
            await ParseNavigationDataAsync(reader, result);
            
            _logger.LogInformation("Parsed RINEX file: {GpsCount} GPS, {GlonassCount} GLONASS, {GalileoCount} Galileo, {BeidouCount} BeiDou satellites",
                result.GpsData.Count, result.GlonassData.Count, result.GalileoData.Count, result.BeidouData.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing RINEX file");
            throw;
        }
        
        return result;
    }

    private async Task<RinexHeader> ParseHeaderAsync(StreamReader reader)
    {
        var header = new RinexHeader();
        string? line;
        
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.Length < 60) continue;
            
            var label = line[60..].Trim();
            var data = line[..60].Trim();
            
            switch (label)
            {
                case "RINEX VERSION / TYPE":
                    ParseVersionType(data, header);
                    break;
                    
                case "PGM / RUN BY / DATE":
                    ParseProgramRunDate(data, header);
                    break;
                    
                case "COMMENT":
                    header.Comments.Add(data);
                    break;
                    
                case "TIME OF FIRST OBS":
                    header.TimeOfFirstObs = ParseDateTime(data);
                    break;
                    
                case "TIME OF LAST OBS":
                    header.TimeOfLastObs = ParseDateTime(data);
                    break;
                    
                case "LEAP SECONDS":
                    if (int.TryParse(data, out var leapSeconds))
                        header.LeapSeconds = leapSeconds;
                    break;
                    
                case "IONOSPHERIC CORR":
                    ParseIonosphericCorr(data, header);
                    break;
                    
                case "TIME SYSTEM CORR":
                    ParseTimeSystemCorr(data, header);
                    break;
                    
                case "END OF HEADER":
                    return header;
            }
        }
        
        throw new InvalidOperationException("RINEX header end marker not found");
    }

    private void ParseVersionType(string data, RinexHeader header)
    {
        var parts = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3)
        {
            header.Version = parts[0];
            header.FileType = parts[1][0];
            header.SatelliteSystem = parts[2][0];
        }
    }

    private void ParseProgramRunDate(string data, RinexHeader header)
    {
        if (data.Length >= 40)
        {
            header.ProgramName = data[..20].Trim();
            header.RunBy = data[20..40].Trim();
            
            if (data.Length >= 60)
            {
                var dateStr = data[40..].Trim();
                if (DateTime.TryParseExact(dateStr, "yyyyMMdd HHmmss UTC", 
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
                {
                    header.CreationDate = date;
                }
            }
        }
    }

    private DateTime? ParseDateTime(string data)
    {
        var parts = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 6 &&
            int.TryParse(parts[0], out var year) &&
            int.TryParse(parts[1], out var month) &&
            int.TryParse(parts[2], out var day) &&
            int.TryParse(parts[3], out var hour) &&
            int.TryParse(parts[4], out var minute) &&
            double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var second))
        {
            return new DateTime(year, month, day, hour, minute, (int)second, (int)((second % 1) * 1000));
        }
        return null;
    }

    private void ParseIonosphericCorr(string data, RinexHeader header)
    {
        var parts = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 5)
        {
            var type = parts[0];
            var values = new double[4];
            for (int i = 0; i < 4 && i + 1 < parts.Length; i++)
            {
                if (double.TryParse(parts[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    values[i] = value;
            }
            header.IonosphericCorr[type] = values;
        }
    }

    private void ParseTimeSystemCorr(string data, RinexHeader header)
    {
        var parts = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 3)
        {
            var type = parts[0];
            var values = new double[2];
            for (int i = 0; i < 2 && i + 1 < parts.Length; i++)
            {
                if (double.TryParse(parts[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    values[i] = value;
            }
            header.TimeSystemCorr[type] = values;
        }
    }

    private async Task ParseNavigationDataAsync(StreamReader reader, RinexParseResult result)
    {
        string? line;
        
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            // Parse satellite identifier from the first character
            var gnssSystem = line[0];
            
            try
            {
                switch (gnssSystem)
                {
                    case 'G':
                        var gpsData = await ParseGpsDataAsync(reader, line);
                        if (gpsData != null)
                            result.GpsData.Add(gpsData);
                        break;
                        
                    case 'R':
                        var glonassData = await ParseGlonassDataAsync(reader, line);
                        if (glonassData != null)
                            result.GlonassData.Add(glonassData);
                        break;
                        
                    case 'E':
                        var galileoData = await ParseGalileoDataAsync(reader, line);
                        if (galileoData != null)
                            result.GalileoData.Add(galileoData);
                        break;
                        
                    case 'C':
                        var beidouData = await ParseBeidouDataAsync(reader, line);
                        if (beidouData != null)
                            result.BeidouData.Add(beidouData);
                        break;
                        
                    default:
                        _logger.LogWarning("Unknown GNSS system: {System}", gnssSystem);
                        await SkipSatelliteDataAsync(reader);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing navigation data for satellite {Satellite}", line[..3]);
                await SkipSatelliteDataAsync(reader);
            }
        }
    }

    private async Task<GpsNavigationData?> ParseGpsDataAsync(StreamReader reader, string firstLine)
    {
        var data = new GpsNavigationData { GnssSystem = 'G' };
        
        // Parse satellite PRN and epoch time from first line
        if (!ParseSatelliteEpoch(firstLine, data))
            return null;
        
        // Parse clock data from first line
        ParseClockData(firstLine, data);
        
        // Read and parse the remaining 7 lines of GPS navigation data
        var lines = new List<string> { firstLine };
        for (int i = 0; i < 7; i++)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) return null;
            lines.Add(line);
        }
        
        // Parse broadcast orbit parameters
        ParseGpsOrbitParameters(lines, data);
        
        data.DownloadTime = DateTime.UtcNow;
        return data;
    }

    private async Task<GlonassNavigationData?> ParseGlonassDataAsync(StreamReader reader, string firstLine)
    {
        var data = new GlonassNavigationData { GnssSystem = 'R' };
        
        // Parse satellite PRN and epoch time from first line
        if (!ParseSatelliteEpoch(firstLine, data))
            return null;
        
        // Parse clock data from first line
        ParseClockData(firstLine, data);
        
        // Read and parse the remaining 3 lines of GLONASS navigation data
        var lines = new List<string> { firstLine };
        for (int i = 0; i < 3; i++)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) return null;
            lines.Add(line);
        }
        
        // Parse satellite position, velocity, and acceleration
        ParseGlonassStateVectors(lines, data);
        
        data.DownloadTime = DateTime.UtcNow;
        return data;
    }

    private async Task<GalileoNavigationData?> ParseGalileoDataAsync(StreamReader reader, string firstLine)
    {
        var data = new GalileoNavigationData { GnssSystem = 'E' };
        
        // Parse satellite PRN and epoch time from first line
        if (!ParseSatelliteEpoch(firstLine, data))
            return null;
        
        // Parse clock data from first line
        ParseClockData(firstLine, data);
        
        // Read and parse the remaining 7 lines of Galileo navigation data
        var lines = new List<string> { firstLine };
        for (int i = 0; i < 7; i++)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) return null;
            lines.Add(line);
        }
        
        // Parse broadcast orbit parameters
        ParseGalileoOrbitParameters(lines, data);
        
        data.DownloadTime = DateTime.UtcNow;
        return data;
    }

    private async Task<BeidouNavigationData?> ParseBeidouDataAsync(StreamReader reader, string firstLine)
    {
        var data = new BeidouNavigationData { GnssSystem = 'C' };
        
        // Parse satellite PRN and epoch time from first line
        if (!ParseSatelliteEpoch(firstLine, data))
            return null;
        
        // Parse clock data from first line
        ParseClockData(firstLine, data);
        
        // Read and parse the remaining 7 lines of BeiDou navigation data
        var lines = new List<string> { firstLine };
        for (int i = 0; i < 7; i++)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) return null;
            lines.Add(line);
        }
        
        // Parse broadcast orbit parameters
        ParseBeidouOrbitParameters(lines, data);
        
        data.DownloadTime = DateTime.UtcNow;
        return data;
    }

    private bool ParseSatelliteEpoch(string line, GnssNavigationData data)
    {
        if (line.Length < 23) return false;
        
        // Parse satellite PRN
        var prnStr = line[1..3].Trim();
        if (!int.TryParse(prnStr, out var prn))
            return false;
        
        data.SatellitePrn = prn;
        
        // Parse epoch time (columns 4-22)
        var epochStr = line[4..23];
        var parts = epochStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length >= 6 &&
            int.TryParse(parts[0], out var year) &&
            int.TryParse(parts[1], out var month) &&
            int.TryParse(parts[2], out var day) &&
            int.TryParse(parts[3], out var hour) &&
            int.TryParse(parts[4], out var minute) &&
            double.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out var second))
        {
            // Handle 2-digit years
            if (year < 100)
                year += year < 80 ? 2000 : 1900;
            
            data.EpochTime = new DateTime(year, month, day, hour, minute, (int)second, 
                (int)((second % 1) * 1000), DateTimeKind.Utc);
            return true;
        }
        
        return false;
    }

    private void ParseClockData(string line, GnssNavigationData data)
    {
        if (line.Length < 80) return;
        
        // Parse clock bias, drift, and drift rate from positions 23-41, 42-60, 61-80
        var values = new[]
        {
            line[23..42].Trim(),
            line[42..61].Trim(),
            line[61..80].Trim()
        };
        
        if (double.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var bias))
            data.ClockBias = bias;
        
        if (double.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var drift))
            data.ClockDrift = drift;
        
        if (double.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var driftRate))
            data.ClockDriftRate = driftRate;
    }

    private void ParseGpsOrbitParameters(List<string> lines, GpsNavigationData data)
    {
        var values = ExtractValuesFromLines(lines, 1, 7); // Skip first line (epoch), parse next 7
        
        if (values.Count >= 28)
        {
            data.Iode = values[0];
            data.Crs = values[1];
            data.DeltaN = values[2];
            data.M0 = values[3];
            
            data.Cuc = values[4];
            data.Eccentricity = values[5];
            data.Cus = values[6];
            data.SqrtA = values[7];
            
            data.Toe = values[8];
            data.Cic = values[9];
            data.Omega0 = values[10];
            data.Cis = values[11];
            
            data.I0 = values[12];
            data.Crc = values[13];
            data.Omega = values[14];
            data.OmegaDot = values[15];
            
            data.Idot = values[16];
            data.CodesOnL2 = values[17];
            data.GpsWeek = values[18];
            data.L2PDataFlag = values[19];
            
            data.SvAccuracy = values[20];
            data.SvHealth = values[21];
            data.Tgd = values[22];
            data.Iodc = values[23];
            
            data.TransmissionTime = values[24];
            if (values.Count > 25)
                data.FitInterval = values[25];
        }
    }

    private void ParseGlonassStateVectors(List<string> lines, GlonassNavigationData data)
    {
        var values = ExtractValuesFromLines(lines, 1, 3); // Skip first line (epoch), parse next 3
        
        if (values.Count >= 12)
        {
            data.PositionX = values[0] * 1000; // Convert km to m
            data.VelocityX = values[1] * 1000; // Convert km/s to m/s
            data.AccelerationX = values[2] * 1000; // Convert km/s² to m/s²
            data.SatelliteHealth = values[3];
            
            data.PositionY = values[4] * 1000;
            data.VelocityY = values[5] * 1000;
            data.AccelerationY = values[6] * 1000;
            data.FrequencyNumber = values[7];
            
            data.PositionZ = values[8] * 1000;
            data.VelocityZ = values[9] * 1000;
            data.AccelerationZ = values[10] * 1000;
            data.AgeOfOperationInfo = values[11];
        }
    }

    private void ParseGalileoOrbitParameters(List<string> lines, GalileoNavigationData data)
    {
        var values = ExtractValuesFromLines(lines, 1, 7); // Skip first line (epoch), parse next 7
        
        if (values.Count >= 28)
        {
            data.Iodnav = values[0];
            data.Crs = values[1];
            data.DeltaN = values[2];
            data.M0 = values[3];
            
            data.Cuc = values[4];
            data.Eccentricity = values[5];
            data.Cus = values[6];
            data.SqrtA = values[7];
            
            data.Toe = values[8];
            data.Cic = values[9];
            data.Omega0 = values[10];
            data.Cis = values[11];
            
            data.I0 = values[12];
            data.Crc = values[13];
            data.Omega = values[14];
            data.OmegaDot = values[15];
            
            data.Idot = values[16];
            data.DataSources = values[17];
            data.GalileoWeek = values[18];
            data.Sisa = values[19];
            
            data.SvHealth = values[20];
            data.BgdE5aE1 = values[21];
            data.BgdE5bE1 = values[22];
            data.TransmissionTime = values[23];
        }
    }

    private void ParseBeidouOrbitParameters(List<string> lines, BeidouNavigationData data)
    {
        var values = ExtractValuesFromLines(lines, 1, 7); // Skip first line (epoch), parse next 7
        
        if (values.Count >= 28)
        {
            data.Aode = values[0];
            data.Crs = values[1];
            data.DeltaN = values[2];
            data.M0 = values[3];
            
            data.Cuc = values[4];
            data.Eccentricity = values[5];
            data.Cus = values[6];
            data.SqrtA = values[7];
            
            data.Toe = values[8];
            data.Cic = values[9];
            data.Omega0 = values[10];
            data.Cis = values[11];
            
            data.I0 = values[12];
            data.Crc = values[13];
            data.Omega = values[14];
            data.OmegaDot = values[15];
            
            data.Idot = values[16];
            data.CodesOnL2 = values[17];
            data.BeidouWeek = values[18];
            data.L2PDataFlag = values[19];
            
            data.SvAccuracy = values[20];
            data.SvHealth = values[21];
            data.Tgd1 = values[22];
            data.Tgd2 = values[23];
            
            data.TransmissionTime = values[24];
            if (values.Count > 25)
                data.Aodc = values[25];
        }
    }

    private List<double> ExtractValuesFromLines(List<string> lines, int startLine, int lineCount)
    {
        var values = new List<double>();
        
        for (int i = startLine; i < startLine + lineCount && i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.Length < 4) continue;
            
            // Each line contains 4 values in scientific notation format
            // Starting from position 4, each value is 19 characters wide
            for (int j = 0; j < 4; j++)
            {
                var start = 4 + j * 19;
                if (start + 19 <= line.Length)
                {
                    var valueStr = line[start..(start + 19)].Trim();
                    if (double.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    {
                        values.Add(value);
                    }
                    else
                    {
                        values.Add(0.0); // Default value for unparseable data
                    }
                }
            }
        }
        
        return values;
    }

    private async Task SkipSatelliteDataAsync(StreamReader reader)
    {
        // Skip the remaining lines for an unknown satellite
        // Most GNSS systems use 7 additional lines after the epoch line
        for (int i = 0; i < 7; i++)
        {
            await reader.ReadLineAsync();
        }
    }
}

/// <summary>
/// Result of RINEX file parsing
/// </summary>
public class RinexParseResult
{
    public RinexHeader Header { get; set; } = new();
    public List<GpsNavigationData> GpsData { get; set; } = new();
    public List<GlonassNavigationData> GlonassData { get; set; } = new();
    public List<GalileoNavigationData> GalileoData { get; set; } = new();
    public List<BeidouNavigationData> BeidouData { get; set; } = new();
}