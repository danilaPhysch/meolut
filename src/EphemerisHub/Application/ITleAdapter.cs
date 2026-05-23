using EphemerisHub.Models;

namespace EphemerisHub.Application;

public interface ITleAdapter
{
    public Task<IReadOnlyList<ParsedTle>> GetTles(CancellationToken cancellationToken = default);
}