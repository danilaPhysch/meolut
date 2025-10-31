namespace EphemerisHub.Infrastructure;

public static class ConfigurationExtensions
{
    public static T GetSettings<T>(this IConfiguration configuration, string section) where T : class =>
        configuration.GetSection(section).Get<T>() ?? throw new InvalidOperationException($"Configuration section '{section}' not found or is empty.");

    public static string GetRequiredConnectionString(this IConfiguration configuration, string name) =>
        configuration.GetConnectionString(name) ?? throw new InvalidOperationException($"Connection string '{name}' not found or is empty.");
}