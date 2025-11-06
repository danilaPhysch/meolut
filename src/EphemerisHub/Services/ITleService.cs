using EphemerisHub.DTOs;

namespace EphemerisHub.Services;

public interface ITleService
{
    Task<IReadOnlyList<TleDto>> GetAllTles(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TleDto>> GetTlesBySystem(string system, CancellationToken cancellationToken = default);
    Task<TleDto?> GetLatestTleBySatellite(string system, int csSatNum, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SatelliteDto>> GetSatellites(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemDto>> GetSystems(CancellationToken cancellationToken = default);
    Task<StatusDto> GetStatus(CancellationToken cancellationToken = default);
}
