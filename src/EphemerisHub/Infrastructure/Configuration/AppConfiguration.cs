using EphemerisHub.Infrastructure.Settings;

namespace EphemerisHub.Infrastructure.Configuration;

public static class AppConfiguration
{
    public static string ConnectionString { get; private set; } = null!;

    public static void RegisterSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetRequiredConnectionString("Postgres");
        var meosarSatellites = configuration.GetSettings<MeosarSatellitesSettings>("MeosarSatellites");
        var tleLoaderSettings = configuration.GetSettings<TleLoaderSettings>("TleLoader");
        var tleSettings = configuration.GetSettings<TleSettings>("Tle");

        ConnectionString = connectionString;

        services.AddSingleton(meosarSatellites);
        services.AddSingleton(tleLoaderSettings);
        services.AddSingleton(tleSettings);
    }
}