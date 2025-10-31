using EphemerisHub.Models;

namespace EphemerisHub.Application;

public interface ITleRepository
{
    Task SaveTles(IReadOnlyList<Tle> tles, CancellationToken cancellationToken);
}