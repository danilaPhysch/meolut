using EphemerisHub.Adapters;
using EphemerisHub.Application;
using EphemerisHub.Infrastructure.Configuration;
using EphemerisHub.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterSettings(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(AppConfiguration.ConnectionString));

builder.Services.AddHttpClient<ITleAdapter, TleAdapter>();
builder.Services.AddScoped<ITleRepository, TleRepository>();

builder.Services.AddHostedService<TleDownloadService>();

var app = builder.Build();

await app.SeedDbContext();
await app.RunAsync();