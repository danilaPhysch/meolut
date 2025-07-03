using EphemerisHub.Infrastructure.Configuration;

namespace EphemerisHub.Infrastructure.Configuration;

public static class AppConfiguration
{
    public static string ConnectionString { get; private set; } = null!;
    public static RinexConfiguration RinexConfig { get; private set; } = null!;

    public static void RegisterSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetRequiredConnectionString("Postgres");
        var rinexConfig = configuration.GetSettings<RinexConfiguration>(RinexConfiguration.Section);

        ConnectionString = connectionString;
        RinexConfig = rinexConfig;
        
        services.AddSingleton(rinexConfig);
    }
}