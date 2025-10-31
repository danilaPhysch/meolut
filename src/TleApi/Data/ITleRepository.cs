using TleApi.DTOs;
using TleApi.Models;

namespace TleApi.Data;

/// <summary>
/// Repository interface for TLE data access
/// </summary>
public interface ITleRepository
{
    Task<PagedResult<TleEntity>> GetTlesAsync(
        string? system = null, 
        string? prn = null, 
        DateTime? datetime = null,
        int page = 1, 
        int pageSize = 50,
        CancellationToken cancellationToken = default);
        
    Task<TleEntity?> GetTleAsync(
        string system, 
        string prn, 
        DateTime? datetime = null,
        CancellationToken cancellationToken = default);
}
