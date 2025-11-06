using EphemerisHub.Adapters;
using EphemerisHub.Application;
using EphemerisHub.Endpoints;
using EphemerisHub.Infrastructure;
using EphemerisHub.Infrastructure.Configuration;
using EphemerisHub.Infrastructure.Database;
using EphemerisHub.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ephemerishub-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.RegisterSettings(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(AppConfiguration.ConnectionString));

// Add Polly policies to HttpClient
builder.Services.AddHttpClient<ITleAdapter, TleAdapter>()
    .AddPolicyHandler(PollyPolicies.GetCombinedPolicy());

builder.Services.AddScoped<ITleRepository, TleRepository>();
builder.Services.AddScoped<ITleService, TleService>();

builder.Services.AddHostedService<TleDownloadService>();
builder.Services.AddHostedService<DataCleanupService>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.SeedDbContext();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSerilogRequestLogging();

// Map health check endpoint
app.MapHealthChecks("/health");

// Map API endpoints
app.MapTleEndpoints();
app.MapSystemEndpoints();

await app.RunAsync();