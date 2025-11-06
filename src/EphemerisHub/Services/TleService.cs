using EphemerisHub.Application;
using EphemerisHub.DTOs;
using EphemerisHub.Models;

namespace EphemerisHub.Services;

public class TleService(ITleRepository tleRepository) : ITleService
{
    public async Task<IReadOnlyList<TleDto>> GetAllTles(CancellationToken cancellationToken = default)
    {
        var tles = await tleRepository.GetAllTles(cancellationToken);
        return tles.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<TleDto>> GetTlesBySystem(string system, CancellationToken cancellationToken = default)
    {
        var tles = await tleRepository.GetTlesBySystem(system, cancellationToken);
        return tles.Select(MapToDto).ToList();
    }

    public async Task<TleDto?> GetLatestTleBySatellite(string system, int csSatNum, CancellationToken cancellationToken = default)
    {
        var tle = await tleRepository.GetLatestTleBySatellite(system, csSatNum, cancellationToken);
        return tle != null ? MapToDto(tle) : null;
    }

    public async Task<IReadOnlyList<SatelliteDto>> GetSatellites(CancellationToken cancellationToken = default)
    {
        var tles = await tleRepository.GetAllTles(cancellationToken);
        return tles
            .GroupBy(t => new { t.CsSatNum, System = GetSystemName(t) })
            .Select(g => new SatelliteDto
            {
                CsSatNum = g.Key.CsSatNum,
                Name = g.First().Name,
                System = g.Key.System,
                LastUpdate = g.Max(t => t.Time)
            })
            .ToList();
    }

    public async Task<IReadOnlyList<SystemDto>> GetSystems(CancellationToken cancellationToken = default)
    {
        var counts = await tleRepository.GetRecordCountsBySystem(cancellationToken);
        
        var systems = new List<SystemDto>
        {
            new() { Name = "GPS", Description = "Global Positioning System (USA)", SatelliteCount = counts.GetValueOrDefault("GPS", 0) },
            new() { Name = "GLONASS", Description = "Global Navigation Satellite System (Russia)", SatelliteCount = counts.GetValueOrDefault("GLONASS", 0) },
            new() { Name = "Galileo", Description = "Galileo (European Union)", SatelliteCount = counts.GetValueOrDefault("Galileo", 0) },
            new() { Name = "BeiDou", Description = "BeiDou Navigation Satellite System (China)", SatelliteCount = counts.GetValueOrDefault("BeiDou", 0) }
        };
        
        return systems;
    }

    public async Task<StatusDto> GetStatus(CancellationToken cancellationToken = default)
    {
        var counts = await tleRepository.GetRecordCountsBySystem(cancellationToken);
        var lastUpdates = await tleRepository.GetLastUpdateBySystem(cancellationToken);

        var systemStatuses = new Dictionary<string, SystemStatusDto>();
        foreach (var system in new[] { "GPS", "Galileo", "GLONASS", "BeiDou" })
        {
            systemStatuses[system] = new SystemStatusDto
            {
                RecordCount = counts.GetValueOrDefault(system, 0),
                LastUpdate = lastUpdates.GetValueOrDefault(system)
            };
        }

        var lastDownload = lastUpdates.Values.Where(v => v.HasValue).DefaultIfEmpty().Max();

        return new StatusDto
        {
            Status = "Running",
            LastDownload = lastDownload,
            Systems = systemStatuses
        };
    }

    private static TleDto MapToDto(Tle tle)
    {
        return new TleDto
        {
            Name = tle.Name,
            Line1 = tle.Line1,
            Line2 = tle.Line2,
            CsSatNum = tle.CsSatNum,
            Time = tle.Time,
            System = GetSystemName(tle)
        };
    }

    private static string GetSystemName(Tle tle)
    {
        return tle switch
        {
            GpsTle => "GPS",
            GalileoTle => "Galileo",
            GlonassTle => "GLONASS",
            BeidouTle => "BeiDou",
            _ => "Unknown"
        };
    }
}
