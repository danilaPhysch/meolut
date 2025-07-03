using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Meolut.Core.Data;
using Meolut.Core.Services;
using Meolut.Core.Parsing;

namespace Meolut.Core;

/// <summary>
/// Extension methods for registering RINEX services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add RINEX services to the service collection
    /// </summary>
    public static IServiceCollection AddRinexServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add database context
        services.AddDbContext<RinexDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Data Source=rinex.db";
            options.UseSqlite(connectionString);
        });

        // Add HTTP client for downloads
        services.AddHttpClient<RinexDownloadService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(120);
            client.DefaultRequestHeaders.Add("User-Agent", "Meolut-RinexClient/1.0");
        });

        // Add core services
        services.AddScoped<RinexParser>();
        services.AddScoped<RinexDataService>();
        services.AddScoped<RinexClientService>();

        // Configure options
        services.Configure<RinexDownloadOptions>(
            configuration.GetSection("RinexDownload"));

        return services;
    }

    /// <summary>
    /// Ensure database is created and up to date
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RinexDbContext>();
        
        await context.Database.EnsureCreatedAsync();
    }
}