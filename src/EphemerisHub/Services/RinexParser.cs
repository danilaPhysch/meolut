using EphemerisHub.Infrastructure.Database;
using EphemerisHub.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EphemerisHub.Services;

public interface IRinexParser
{
    Task ParseAndSaveAsync(string filePath, CancellationToken cancellationToken = default);
}

public class RinexParser : IRinexParser
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RinexParser> _logger;

    public RinexParser(AppDbContext dbContext, ILogger<RinexParser> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ParseAndSaveAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"RINEX file not found: {filePath}");
        }

        _logger.LogInformation("Parsing RINEX file: {FilePath}", filePath);

        using var reader = new StreamReader(filePath);
        var header = await ParseHeaderAsync(reader, cancellationToken);
        
        if (header.FileType != "NAVIGATION")
        {
            _logger.LogWarning("File {FilePath} is not a navigation file, skipping", filePath);
            return;
        }

        var ephemerisList = new List<RinexEphemeris>();
        string? line;
        
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
                
            if (IsEphemerisLine(line))
            {
                var ephemeris = await ParseEphemerisBlockAsync(reader, line, header.Version, cancellationToken);
                if (ephemeris != null)
                {
                    ephemerisList.Add(ephemeris);
                }
            }
        }

        await SaveEphemerisDataAsync(ephemerisList, cancellationToken);
        
        _logger.LogInformation("Successfully parsed {Count} ephemeris records from {FilePath}", 
            ephemerisList.Count, filePath);
    }

    private async Task<RinexHeader> ParseHeaderAsync(StreamReader reader, CancellationToken cancellationToken)
    {
        var header = new RinexHeader();
        string? line;
        
        while ((line = await reader.ReadLineAsync()) != null && !line.Contains("END OF HEADER"))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
                
            if (line.Contains("RINEX VERSION / TYPE"))
            {
                header.Version = double.Parse(line.Substring(0, 9).Trim(), CultureInfo.InvariantCulture);
                header.FileType = line.Substring(20, 20).Trim();
            }
        }
        
        return header;
    }

    private static bool IsEphemerisLine(string line)
    {
        if (line.Length < 3) return false;
        
        // Check for satellite system identifier (G, R, E, C) in first character
        var satelliteSystem = line[0];
        return satelliteSystem is 'G' or 'R' or 'E' or 'C';
    }

    private async Task<RinexEphemeris?> ParseEphemerisBlockAsync(StreamReader reader, string firstLine, 
        double version, CancellationToken cancellationToken)
    {
        var satelliteSystem = firstLine[0];
        
        try
        {
            return satelliteSystem switch
            {
                'G' => await ParseGpsEphemerisAsync(reader, firstLine, version, cancellationToken),
                'R' => await ParseGlonassEphemerisAsync(reader, firstLine, version, cancellationToken),
                'E' => await ParseGalileoEphemerisAsync(reader, firstLine, version, cancellationToken),
                'C' => await ParseBeidouEphemerisAsync(reader, firstLine, version, cancellationToken),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse ephemeris for satellite system {System}", satelliteSystem);
            return null;
        }
    }

    private async Task<RinexGpsEphemeris?> ParseGpsEphemerisAsync(StreamReader reader, string firstLine, 
        double version, CancellationToken cancellationToken)
    {
        var ephemeris = new RinexGpsEphemeris { SatelliteSystem = "G" };
        
        // Parse first line (satellite info and clock data)
        if (!ParseSatelliteAndClockData(firstLine, ephemeris))
            return null;

        // Read and parse the 7 data lines for GPS
        var lines = new List<string>();
        for (int i = 0; i < 7; i++)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) return null;
            lines.Add(line);
        }

        if (lines.Count < 7) return null;

        try
        {
            // Line 1: IODE, Crs, Delta n, M0
            var values1 = ParseDataLine(lines[0]);
            ephemeris.Iode = values1[0];
            ephemeris.Crs = values1[1];
            ephemeris.DeltaN = values1[2];
            ephemeris.M0 = values1[3];

            // Line 2: Cuc, e, Cus, sqrt(A)
            var values2 = ParseDataLine(lines[1]);
            ephemeris.Cuc = values2[0];
            ephemeris.Eccentricity = values2[1];
            ephemeris.Cus = values2[2];
            ephemeris.SqrtA = values2[3];

            // Line 3: TOE, Cic, OMEGA0, Cis
            var values3 = ParseDataLine(lines[2]);
            ephemeris.Toe = values3[0];
            ephemeris.Cic = values3[1];
            ephemeris.Omega0 = values3[2];
            ephemeris.Cis = values3[3];

            // Line 4: i0, Crc, omega, OMEGA DOT
            var values4 = ParseDataLine(lines[3]);
            ephemeris.I0 = values4[0];
            ephemeris.Crc = values4[1];
            ephemeris.Omega = values4[2];
            ephemeris.OmegaDot = values4[3];

            // Line 5: IDOT, Codes on L2, GPS Week, L2 P data flag
            var values5 = ParseDataLine(lines[4]);
            ephemeris.Idot = values5[0];
            ephemeris.CodesOnL2 = values5[1];
            ephemeris.GpsWeek = values5[2];
            ephemeris.L2PDataFlag = values5[3];

            // Line 6: SV accuracy, SV health, TGD, IODC
            var values6 = ParseDataLine(lines[5]);
            ephemeris.SvAccuracy = values6[0];
            ephemeris.SvHealth = values6[1];
            ephemeris.Tgd = values6[2];
            ephemeris.Iodc = values6[3];

            // Line 7: Transmission time, Fit interval
            var values7 = ParseDataLine(lines[6]);
            ephemeris.TransmissionTime = values7[0];
            if (values7.Length > 1)
                ephemeris.FitInterval = values7[1];

            return ephemeris;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse GPS ephemeris data");
            return null;
        }
    }

    private async Task<RinexGlonassEphemeris?> ParseGlonassEphemerisAsync(StreamReader reader, string firstLine, 
        double version, CancellationToken cancellationToken)
    {
        // Similar implementation for GLONASS - simplified for now
        var ephemeris = new RinexGlonassEphemeris { SatelliteSystem = "R" };
        
        if (!ParseSatelliteAndClockData(firstLine, ephemeris))
            return null;

        // Skip the data lines for now (would need GLONASS-specific parsing)
        for (int i = 0; i < 3; i++)
        {
            await reader.ReadLineAsync();
        }

        return ephemeris;
    }

    private async Task<RinexGalileoEphemeris?> ParseGalileoEphemerisAsync(StreamReader reader, string firstLine, 
        double version, CancellationToken cancellationToken)
    {
        // Similar implementation for Galileo - simplified for now
        var ephemeris = new RinexGalileoEphemeris { SatelliteSystem = "E" };
        
        if (!ParseSatelliteAndClockData(firstLine, ephemeris))
            return null;

        // Skip the data lines for now (would need Galileo-specific parsing)
        for (int i = 0; i < 7; i++)
        {
            await reader.ReadLineAsync();
        }

        return ephemeris;
    }

    private async Task<RinexBeidouEphemeris?> ParseBeidouEphemerisAsync(StreamReader reader, string firstLine, 
        double version, CancellationToken cancellationToken)
    {
        // Similar implementation for BeiDou - simplified for now
        var ephemeris = new RinexBeidouEphemeris { SatelliteSystem = "C" };
        
        if (!ParseSatelliteAndClockData(firstLine, ephemeris))
            return null;

        // Skip the data lines for now (would need BeiDou-specific parsing)
        for (int i = 0; i < 7; i++)
        {
            await reader.ReadLineAsync();
        }

        return ephemeris;
    }

    private static bool ParseSatelliteAndClockData(string line, RinexEphemeris ephemeris)
    {
        try
        {
            // Extract PRN number
            var prnStr = line.Substring(1, 2).Trim();
            if (!int.TryParse(prnStr, out var prn))
                return false;
            ephemeris.SatellitePrn = prn;

            // Extract time of clock
            var year = int.Parse(line.Substring(4, 4));
            var month = int.Parse(line.Substring(9, 2));
            var day = int.Parse(line.Substring(12, 2));
            var hour = int.Parse(line.Substring(15, 2));
            var minute = int.Parse(line.Substring(18, 2));
            var second = int.Parse(line.Substring(21, 2));
            
            ephemeris.TimeOfClock = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

            // Extract clock coefficients
            var clockData = line.Substring(23);
            var clockValues = ParseDataLine(clockData);
            
            if (clockValues.Length >= 3)
            {
                ephemeris.ClockBias = clockValues[0];
                ephemeris.ClockDrift = clockValues[1];
                ephemeris.ClockDriftRate = clockValues[2];
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static double[] ParseDataLine(string line)
    {
        var values = new List<double>();
        var regex = new Regex(@"([+-]?\d+\.\d+[eE][+-]?\d+)|([+-]?\d+\.\d+)|([+-]?\d+)");
        var matches = regex.Matches(line);
        
        foreach (Match match in matches)
        {
            if (double.TryParse(match.Value.Replace("D", "E"), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                values.Add(value);
            }
        }
        
        return values.ToArray();
    }

    private async Task SaveEphemerisDataAsync(List<RinexEphemeris> ephemerisList, CancellationToken cancellationToken)
    {
        foreach (var ephemeris in ephemerisList)
        {
            try
            {
                switch (ephemeris)
                {
                    case RinexGpsEphemeris gps:
                        if (!await _dbContext.GpsEphemeris.AnyAsync(e => 
                            e.SatellitePrn == gps.SatellitePrn && e.TimeOfClock == gps.TimeOfClock, cancellationToken))
                        {
                            _dbContext.GpsEphemeris.Add(gps);
                        }
                        break;
                    case RinexGlonassEphemeris glonass:
                        if (!await _dbContext.GlonassEphemeris.AnyAsync(e => 
                            e.SatellitePrn == glonass.SatellitePrn && e.TimeOfClock == glonass.TimeOfClock, cancellationToken))
                        {
                            _dbContext.GlonassEphemeris.Add(glonass);
                        }
                        break;
                    case RinexGalileoEphemeris galileo:
                        if (!await _dbContext.GalileoEphemeris.AnyAsync(e => 
                            e.SatellitePrn == galileo.SatellitePrn && e.TimeOfClock == galileo.TimeOfClock, cancellationToken))
                        {
                            _dbContext.GalileoEphemeris.Add(galileo);
                        }
                        break;
                    case RinexBeidouEphemeris beidou:
                        if (!await _dbContext.BeidouEphemeris.AnyAsync(e => 
                            e.SatellitePrn == beidou.SatellitePrn && e.TimeOfClock == beidou.TimeOfClock, cancellationToken))
                        {
                            _dbContext.BeidouEphemeris.Add(beidou);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save ephemeris data for satellite {System}{Prn}", 
                    ephemeris.SatelliteSystem, ephemeris.SatellitePrn);
            }
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save ephemeris data to database");
            throw;
        }
    }

    private class RinexHeader
    {
        public double Version { get; set; }
        public string FileType { get; set; } = string.Empty;
    }
}