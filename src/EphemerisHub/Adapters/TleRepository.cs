using EphemerisHub.Application;
using EphemerisHub.Infrastructure.Database;
using EphemerisHub.Models;
using Microsoft.EntityFrameworkCore;

namespace EphemerisHub.Adapters;

public class TleRepository(AppDbContext appDbContext) : ITleRepository
{
    public async Task SaveTles(IReadOnlyList<Tle> tles, CancellationToken cancellationToken)
    {
        await appDbContext.AddRangeAsync(tles, cancellationToken);
        await appDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tle>> GetAllTles(CancellationToken cancellationToken = default)
    {
        var gps = await appDbContext.GpsTle.ToListAsync(cancellationToken);
        var galileo = await appDbContext.GalileoTle.ToListAsync(cancellationToken);
        var glonass = await appDbContext.GlonassTle.ToListAsync(cancellationToken);
        var beidou = await appDbContext.BeidouTle.ToListAsync(cancellationToken);
        
        return gps.Cast<Tle>()
            .Concat(galileo)
            .Concat(glonass)
            .Concat(beidou)
            .ToList();
    }

    public async Task<IReadOnlyList<Tle>> GetTlesBySystem(string system, CancellationToken cancellationToken = default)
    {
        return system.ToUpperInvariant() switch
        {
            "GPS" => (await appDbContext.GpsTle.ToListAsync(cancellationToken)).Cast<Tle>().ToList(),
            "GALILEO" => (await appDbContext.GalileoTle.ToListAsync(cancellationToken)).Cast<Tle>().ToList(),
            "GLONASS" => (await appDbContext.GlonassTle.ToListAsync(cancellationToken)).Cast<Tle>().ToList(),
            "BEIDOU" => (await appDbContext.BeidouTle.ToListAsync(cancellationToken)).Cast<Tle>().ToList(),
            _ => []
        };
    }

    public async Task<Tle?> GetLatestTleBySatellite(string system, int csSatNum, CancellationToken cancellationToken = default)
    {
        return system.ToUpperInvariant() switch
        {
            "GPS" => await appDbContext.GpsTle
                .Where(t => t.CsSatNum == csSatNum)
                .OrderByDescending(t => t.Time)
                .FirstOrDefaultAsync(cancellationToken),
            "GALILEO" => await appDbContext.GalileoTle
                .Where(t => t.CsSatNum == csSatNum)
                .OrderByDescending(t => t.Time)
                .FirstOrDefaultAsync(cancellationToken),
            "GLONASS" => await appDbContext.GlonassTle
                .Where(t => t.CsSatNum == csSatNum)
                .OrderByDescending(t => t.Time)
                .FirstOrDefaultAsync(cancellationToken),
            "BEIDOU" => await appDbContext.BeidouTle
                .Where(t => t.CsSatNum == csSatNum)
                .OrderByDescending(t => t.Time)
                .FirstOrDefaultAsync(cancellationToken),
            _ => null
        };
    }

    public async Task<Dictionary<string, int>> GetRecordCountsBySystem(CancellationToken cancellationToken = default)
    {
        return new Dictionary<string, int>
        {
            ["GPS"] = await appDbContext.GpsTle.CountAsync(cancellationToken),
            ["Galileo"] = await appDbContext.GalileoTle.CountAsync(cancellationToken),
            ["GLONASS"] = await appDbContext.GlonassTle.CountAsync(cancellationToken),
            ["BeiDou"] = await appDbContext.BeidouTle.CountAsync(cancellationToken)
        };
    }

    public async Task<Dictionary<string, DateTime?>> GetLastUpdateBySystem(CancellationToken cancellationToken = default)
    {
        return new Dictionary<string, DateTime?>
        {
            ["GPS"] = await appDbContext.GpsTle.MaxAsync(t => (DateTime?)t.Time, cancellationToken),
            ["Galileo"] = await appDbContext.GalileoTle.MaxAsync(t => (DateTime?)t.Time, cancellationToken),
            ["GLONASS"] = await appDbContext.GlonassTle.MaxAsync(t => (DateTime?)t.Time, cancellationToken),
            ["BeiDou"] = await appDbContext.BeidouTle.MaxAsync(t => (DateTime?)t.Time, cancellationToken)
        };
    }

    public async Task DeleteOldTles(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        await appDbContext.GpsTle.Where(t => t.Time < cutoffDate).ExecuteDeleteAsync(cancellationToken);
        await appDbContext.GalileoTle.Where(t => t.Time < cutoffDate).ExecuteDeleteAsync(cancellationToken);
        await appDbContext.GlonassTle.Where(t => t.Time < cutoffDate).ExecuteDeleteAsync(cancellationToken);
        await appDbContext.BeidouTle.Where(t => t.Time < cutoffDate).ExecuteDeleteAsync(cancellationToken);
    }
}