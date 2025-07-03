using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Meolut.Core.Data;
using Meolut.Core.Models;
using Meolut.Core.Parsing;

namespace Meolut.Core.Services;

/// <summary>
/// Service for managing RINEX navigation data in the database
/// </summary>
public class RinexDataService
{
    private readonly RinexDbContext _dbContext;
    private readonly ILogger<RinexDataService> _logger;

    public RinexDataService(RinexDbContext dbContext, ILogger<RinexDataService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Save parsed RINEX data to the database
    /// </summary>
    /// <param name="parseResult">Parsed RINEX data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of records saved</returns>
    public async Task<int> SaveRinexDataAsync(RinexParseResult parseResult, CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var totalSaved = 0;

            // Save GPS data
            if (parseResult.GpsData.Any())
            {
                var gpsSaved = await SaveGpsDataAsync(parseResult.GpsData, cancellationToken);
                totalSaved += gpsSaved;
                _logger.LogInformation("Saved {Count} GPS navigation records", gpsSaved);
            }

            // Save GLONASS data
            if (parseResult.GlonassData.Any())
            {
                var glonassSaved = await SaveGlonassDataAsync(parseResult.GlonassData, cancellationToken);
                totalSaved += glonassSaved;
                _logger.LogInformation("Saved {Count} GLONASS navigation records", glonassSaved);
            }

            // Save Galileo data
            if (parseResult.GalileoData.Any())
            {
                var galileoSaved = await SaveGalileoDataAsync(parseResult.GalileoData, cancellationToken);
                totalSaved += galileoSaved;
                _logger.LogInformation("Saved {Count} Galileo navigation records", galileoSaved);
            }

            // Save BeiDou data
            if (parseResult.BeidouData.Any())
            {
                var beidouSaved = await SaveBeidouDataAsync(parseResult.BeidouData, cancellationToken);
                totalSaved += beidouSaved;
                _logger.LogInformation("Saved {Count} BeiDou navigation records", beidouSaved);
            }

            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Successfully saved {TotalCount} navigation records to database", totalSaved);
            
            return totalSaved;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error saving RINEX data to database");
            throw;
        }
    }

    /// <summary>
    /// Get GPS navigation data for a specific time range
    /// </summary>
    public async Task<List<GpsNavigationData>> GetGpsDataAsync(DateTime startTime, DateTime endTime, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GpsNavigationData
            .Where(x => x.EpochTime >= startTime && x.EpochTime <= endTime)
            .OrderBy(x => x.EpochTime)
            .ThenBy(x => x.SatellitePrn)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get GLONASS navigation data for a specific time range
    /// </summary>
    public async Task<List<GlonassNavigationData>> GetGlonassDataAsync(DateTime startTime, DateTime endTime, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GlonassNavigationData
            .Where(x => x.EpochTime >= startTime && x.EpochTime <= endTime)
            .OrderBy(x => x.EpochTime)
            .ThenBy(x => x.SatellitePrn)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get Galileo navigation data for a specific time range
    /// </summary>
    public async Task<List<GalileoNavigationData>> GetGalileoDataAsync(DateTime startTime, DateTime endTime, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GalileoNavigationData
            .Where(x => x.EpochTime >= startTime && x.EpochTime <= endTime)
            .OrderBy(x => x.EpochTime)
            .ThenBy(x => x.SatellitePrn)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get BeiDou navigation data for a specific time range
    /// </summary>
    public async Task<List<BeidouNavigationData>> GetBeidouDataAsync(DateTime startTime, DateTime endTime, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BeidouNavigationData
            .Where(x => x.EpochTime >= startTime && x.EpochTime <= endTime)
            .OrderBy(x => x.EpochTime)
            .ThenBy(x => x.SatellitePrn)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Check if data exists for a specific date
    /// </summary>
    public async Task<bool> HasDataForDateAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var startTime = date.ToDateTime(TimeOnly.MinValue);
        var endTime = date.ToDateTime(TimeOnly.MaxValue);

        var hasGps = await _dbContext.GpsNavigationData
            .AnyAsync(x => x.EpochTime >= startTime && x.EpochTime <= endTime, cancellationToken);

        var hasGlonass = await _dbContext.GlonassNavigationData
            .AnyAsync(x => x.EpochTime >= startTime && x.EpochTime <= endTime, cancellationToken);

        var hasGalileo = await _dbContext.GalileoNavigationData
            .AnyAsync(x => x.EpochTime >= startTime && x.EpochTime <= endTime, cancellationToken);

        var hasBeidou = await _dbContext.BeidouNavigationData
            .AnyAsync(x => x.EpochTime >= startTime && x.EpochTime <= endTime, cancellationToken);

        return hasGps || hasGlonass || hasGalileo || hasBeidou;
    }

    /// <summary>
    /// Delete data older than specified date
    /// </summary>
    public async Task<int> DeleteOldDataAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var deletedCount = 0;

            deletedCount += await _dbContext.GpsNavigationData
                .Where(x => x.EpochTime < olderThan)
                .ExecuteDeleteAsync(cancellationToken);

            deletedCount += await _dbContext.GlonassNavigationData
                .Where(x => x.EpochTime < olderThan)
                .ExecuteDeleteAsync(cancellationToken);

            deletedCount += await _dbContext.GalileoNavigationData
                .Where(x => x.EpochTime < olderThan)
                .ExecuteDeleteAsync(cancellationToken);

            deletedCount += await _dbContext.BeidouNavigationData
                .Where(x => x.EpochTime < olderThan)
                .ExecuteDeleteAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Deleted {Count} old navigation records before {Date}", deletedCount, olderThan);
            
            return deletedCount;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error deleting old data");
            throw;
        }
    }

    private async Task<int> SaveGpsDataAsync(List<GpsNavigationData> data, CancellationToken cancellationToken)
    {
        var saved = 0;
        
        foreach (var item in data)
        {
            if (await IsValidGpsData(item) && !await DuplicateExists(item, cancellationToken))
            {
                _dbContext.GpsNavigationData.Add(item);
                saved++;
            }
        }
        
        if (saved > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);
        
        return saved;
    }

    private async Task<int> SaveGlonassDataAsync(List<GlonassNavigationData> data, CancellationToken cancellationToken)
    {
        var saved = 0;
        
        foreach (var item in data)
        {
            if (await IsValidGlonassData(item) && !await DuplicateExists(item, cancellationToken))
            {
                _dbContext.GlonassNavigationData.Add(item);
                saved++;
            }
        }
        
        if (saved > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);
        
        return saved;
    }

    private async Task<int> SaveGalileoDataAsync(List<GalileoNavigationData> data, CancellationToken cancellationToken)
    {
        var saved = 0;
        
        foreach (var item in data)
        {
            if (await IsValidGalileoData(item) && !await DuplicateExists(item, cancellationToken))
            {
                _dbContext.GalileoNavigationData.Add(item);
                saved++;
            }
        }
        
        if (saved > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);
        
        return saved;
    }

    private async Task<int> SaveBeidouDataAsync(List<BeidouNavigationData> data, CancellationToken cancellationToken)
    {
        var saved = 0;
        
        foreach (var item in data)
        {
            if (await IsValidBeidouData(item) && !await DuplicateExists(item, cancellationToken))
            {
                _dbContext.BeidouNavigationData.Add(item);
                saved++;
            }
        }
        
        if (saved > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);
        
        return saved;
    }

    private async Task<bool> DuplicateExists(GnssNavigationData data, CancellationToken cancellationToken)
    {
        return data switch
        {
            GpsNavigationData gps => await _dbContext.GpsNavigationData
                .AnyAsync(x => x.SatellitePrn == gps.SatellitePrn && x.EpochTime == gps.EpochTime, cancellationToken),
            
            GlonassNavigationData glonass => await _dbContext.GlonassNavigationData
                .AnyAsync(x => x.SatellitePrn == glonass.SatellitePrn && x.EpochTime == glonass.EpochTime, cancellationToken),
            
            GalileoNavigationData galileo => await _dbContext.GalileoNavigationData
                .AnyAsync(x => x.SatellitePrn == galileo.SatellitePrn && x.EpochTime == galileo.EpochTime, cancellationToken),
            
            BeidouNavigationData beidou => await _dbContext.BeidouNavigationData
                .AnyAsync(x => x.SatellitePrn == beidou.SatellitePrn && x.EpochTime == beidou.EpochTime, cancellationToken),
            
            _ => false
        };
    }

    private Task<bool> IsValidGpsData(GpsNavigationData data)
    {
        // Basic validation for GPS data
        return Task.FromResult(
            data.SatellitePrn > 0 && data.SatellitePrn <= 32 &&
            data.EpochTime != default &&
            !double.IsNaN(data.ClockBias) &&
            !double.IsInfinity(data.ClockBias)
        );
    }

    private Task<bool> IsValidGlonassData(GlonassNavigationData data)
    {
        // Basic validation for GLONASS data
        return Task.FromResult(
            data.SatellitePrn > 0 && data.SatellitePrn <= 24 &&
            data.EpochTime != default &&
            !double.IsNaN(data.ClockBias) &&
            !double.IsInfinity(data.ClockBias)
        );
    }

    private Task<bool> IsValidGalileoData(GalileoNavigationData data)
    {
        // Basic validation for Galileo data
        return Task.FromResult(
            data.SatellitePrn > 0 && data.SatellitePrn <= 36 &&
            data.EpochTime != default &&
            !double.IsNaN(data.ClockBias) &&
            !double.IsInfinity(data.ClockBias)
        );
    }

    private Task<bool> IsValidBeidouData(BeidouNavigationData data)
    {
        // Basic validation for BeiDou data
        return Task.FromResult(
            data.SatellitePrn > 0 && data.SatellitePrn <= 63 &&
            data.EpochTime != default &&
            !double.IsNaN(data.ClockBias) &&
            !double.IsInfinity(data.ClockBias)
        );
    }
}