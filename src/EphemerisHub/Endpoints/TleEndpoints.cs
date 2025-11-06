using EphemerisHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace EphemerisHub.Endpoints;

public static class TleEndpoints
{
    public static void MapTleEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tle").WithTags("TLE");

        group.MapGet("/", async ([FromServices] ITleService tleService, CancellationToken cancellationToken) =>
        {
            var tles = await tleService.GetAllTles(cancellationToken);
            return Results.Ok(tles);
        })
        .WithName("GetAllTles")
        .WithOpenApi();

        group.MapGet("/{system}", async (string system, [FromServices] ITleService tleService, CancellationToken cancellationToken) =>
        {
            var tles = await tleService.GetTlesBySystem(system, cancellationToken);
            if (!tles.Any())
            {
                return Results.NotFound(new { message = $"No TLE data found for system: {system}" });
            }
            return Results.Ok(tles);
        })
        .WithName("GetTlesBySystem")
        .WithOpenApi();

        group.MapGet("/{system}/{satnum:int}", async (string system, int satnum, [FromServices] ITleService tleService, CancellationToken cancellationToken) =>
        {
            var tle = await tleService.GetLatestTleBySatellite(system, satnum, cancellationToken);
            if (tle == null)
            {
                return Results.NotFound(new { message = $"No TLE data found for satellite {satnum} in system {system}" });
            }
            return Results.Ok(tle);
        })
        .WithName("GetLatestTleBySatellite")
        .WithOpenApi();
    }
}
