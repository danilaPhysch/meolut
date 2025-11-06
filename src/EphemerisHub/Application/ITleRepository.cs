using EphemerisHub.Models;

namespace EphemerisHub.Application;

public interface ITleRepository
{
    Task SaveTles(IReadOnlyList<Tle> tles, CancellationToken cancellationToken);
    Task<IReadOnlyList<Tle>> GetAllTles(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tle>> GetTlesBySystem(string system, CancellationToken cancellationToken = default);
    Task<Tle?> GetLatestTleBySatellite(string system, int csSatNum, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetRecordCountsBySystem(CancellationToken cancellationToken = default);
    Task<Dictionary<string, DateTime?>> GetLastUpdateBySystem(CancellationToken cancellationToken = default);
    Task DeleteOldTles(DateTime cutoffDate, CancellationToken cancellationToken = default);
}