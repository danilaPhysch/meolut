namespace EphemerisHub.Infrastructure.Configuration;

public static class AppConfiguration
{
    public static string ConnectionString { get; private set; } = null!;

    public static void RegisterSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetRequiredConnectionString("Postgres");

        ConnectionString = connectionString;
    }
}