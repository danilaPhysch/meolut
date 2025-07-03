using Microsoft.EntityFrameworkCore;

namespace EphemerisHub.Infrastructure.Database;

public static class AppDbContextSeed
{
    public static async Task SeedDbContext(this IHost host)
    {
        await using var scope = host.Services.CreateAsyncScope();

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Exception exception)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
            logger.LogError(exception, "An error occurred while migrating the database.");
            throw;
        }
    }
}