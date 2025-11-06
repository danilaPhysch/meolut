using EphemerisHub.Services;
using Microsoft.AspNetCore.Mvc;

namespace EphemerisHub.Endpoints;

public static class SystemEndpoints
{
    public static void MapSystemEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api").WithTags("System");

        group.MapGet("/satellites", async ([FromServices] ITleService tleService, CancellationToken cancellationToken) =>
        {
            var satellites = await tleService.GetSatellites(cancellationToken);
            return Results.Ok(satellites);
        })
        .WithName("GetSatellites")
        .WithOpenApi();

        group.MapGet("/systems", async ([FromServices] ITleService tleService, CancellationToken cancellationToken) =>
        {
            var systems = await tleService.GetSystems(cancellationToken);
            return Results.Ok(systems);
        })
        .WithName("GetSystems")
        .WithOpenApi();

        group.MapGet("/status", async ([FromServices] ITleService tleService, CancellationToken cancellationToken) =>
        {
            var status = await tleService.GetStatus(cancellationToken);
            return Results.Ok(status);
        })
        .WithName("GetStatus")
        .WithOpenApi();
    }
}
