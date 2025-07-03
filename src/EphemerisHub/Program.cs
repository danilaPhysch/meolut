using EphemerisHub.Infrastructure.Configuration;
using EphemerisHub.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterSettings(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(AppConfiguration.ConnectionString));

var app = builder.Build();

await app.SeedDbContext();
await app.RunAsync();
