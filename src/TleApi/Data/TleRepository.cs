using Microsoft.EntityFrameworkCore;
using TleApi.DTOs;
using TleApi.Models;

namespace TleApi.Data;

/// <summary>
/// Repository implementation for TLE data access
/// </summary>
public class TleRepository : ITleRepository
{
    private readonly TleDbContext _context;

    public TleRepository(TleDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TleEntity>> GetTlesAsync(
        string? system = null,
        string? prn = null,
        DateTime? datetime = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TleData.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(system))
        {
            query = query.Where(t => t.System == system);
        }

        if (!string.IsNullOrWhiteSpace(prn))
        {
            query = query.Where(t => t.Prn == prn);
        }

        if (datetime.HasValue)
        {
            // Get records with epoch <= datetime, ordered by epoch descending
            query = query.Where(t => t.Epoch <= datetime.Value);
        }

        // Get total count before pagination
        var total = await query.CountAsync(cancellationToken);

        // Order by epoch descending to get most recent first
        query = query.OrderByDescending(t => t.Epoch);

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TleEntity>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<TleEntity?> GetTleAsync(
        string system,
        string prn,
        DateTime? datetime = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TleData
            .Where(t => t.System == system && t.Prn == prn);

        if (datetime.HasValue)
        {
            // Get the most recent record with epoch <= datetime
            query = query.Where(t => t.Epoch <= datetime.Value);
        }

        // Order by epoch descending and take the first (most recent)
        return await query
            .OrderByDescending(t => t.Epoch)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
