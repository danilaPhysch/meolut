using EphemerisHub.Application;
using EphemerisHub.Infrastructure.Database;
using EphemerisHub.Models;

namespace EphemerisHub.Adapters;

public class TleRepository(AppDbContext appDbContext) : ITleRepository
{
    public async Task SaveTles(IReadOnlyList<Tle> tles, CancellationToken cancellationToken)
    {
        await appDbContext.AddRangeAsync(tles, cancellationToken);
        await appDbContext.SaveChangesAsync(cancellationToken);
    }
}