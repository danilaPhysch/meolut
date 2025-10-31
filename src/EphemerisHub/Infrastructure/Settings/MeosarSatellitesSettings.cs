namespace EphemerisHub.Infrastructure.Settings;

public class MeosarSatellitesSettings
{
    public required Dictionary<int, int> GlonassNoradIds { get; init; }
    public required Dictionary<int, int> GpsNoradIds { get; init; }
    public required Dictionary<int, int> GalileoNoradIds { get; init; }
    public required Dictionary<int, int> BeiDouNoradIds { get; init; }

    public Dictionary<int, int> AllNoradIds =>
        GlonassNoradIds
            .Concat(GpsNoradIds)
            .Concat(GalileoNoradIds)
            .Concat(BeiDouNoradIds)
            .ToDictionary(x => x.Key, x => x.Value);
}
