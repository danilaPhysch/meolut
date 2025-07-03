using EphemerisHub.Infrastructure.Configuration;
using EphemerisHub.Infrastructure.Database;
using EphemerisHub.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EphemerisHub;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup Serilog
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/rinex-client.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            Log.Information("Starting RINEX Client application");

            var builder = Host.CreateApplicationBuilder(args);

            // Configure services
            builder.Services.AddSerilog();
            builder.Services.RegisterSettings(builder.Configuration);

            // Add Entity Framework
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(AppConfiguration.ConnectionString));

            // Add HTTP client
            builder.Services.AddHttpClient<IRinexDownloader, RinexDownloader>();

            // Add services
            builder.Services.AddScoped<IRinexDownloader, RinexDownloader>();
            builder.Services.AddScoped<IRinexParser, RinexParser>();
            builder.Services.AddHostedService<RinexSchedulerService>();

            var host = builder.Build();

            // Initialize database
            await host.SeedDbContext();

            // Handle command line arguments
            if (args.Length > 0)
            {
                await HandleCommandLineAsync(host, args);
            }
            else
            {
                // Run as background service
                await host.RunAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static async Task HandleCommandLineAsync(IHost host, string[] args)
    {
        using var scope = host.Services.CreateScope();
        var downloader = scope.ServiceProvider.GetRequiredService<IRinexDownloader>();
        var parser = scope.ServiceProvider.GetRequiredService<IRinexParser>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        var command = args[0].ToLower();

        switch (command)
        {
            case "download":
                await HandleDownloadCommand(downloader, parser, logger, args);
                break;
            case "parse":
                await HandleParseCommand(parser, logger, args);
                break;
            case "help":
            case "--help":
            case "-h":
                ShowHelp();
                break;
            default:
                logger.LogError("Unknown command: {Command}. Use 'help' for available commands.", command);
                break;
        }
    }

    private static async Task HandleDownloadCommand(IRinexDownloader downloader, IRinexParser parser, 
        ILogger<Program> logger, string[] args)
    {
        var date = DateTime.UtcNow.Date;
        var parseFiles = true;

        // Parse additional arguments
        for (int i = 1; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "--date":
                    if (i + 1 < args.Length && DateTime.TryParse(args[i + 1], out var parsedDate))
                    {
                        date = parsedDate.Date;
                        i++; // Skip next argument
                    }
                    break;
                case "--no-parse":
                    parseFiles = false;
                    break;
            }
        }

        logger.LogInformation("Downloading RINEX files for date: {Date:yyyy-MM-dd}", date);

        try
        {
            var downloadedFiles = await downloader.DownloadDailyFilesAsync(date);
            logger.LogInformation("Downloaded {Count} files", downloadedFiles.Length);

            if (parseFiles)
            {
                foreach (var filePath in downloadedFiles)
                {
                    try
                    {
                        await parser.ParseAndSaveAsync(filePath);
                        logger.LogInformation("Successfully parsed: {FilePath}", filePath);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to parse: {FilePath}", filePath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to download files for date: {Date:yyyy-MM-dd}", date);
        }
    }

    private static async Task HandleParseCommand(IRinexParser parser, ILogger<Program> logger, string[] args)
    {
        if (args.Length < 2)
        {
            logger.LogError("Parse command requires a file path. Usage: parse <file-path>");
            return;
        }

        var filePath = args[1];
        
        try
        {
            await parser.ParseAndSaveAsync(filePath);
            logger.LogInformation("Successfully parsed: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse: {FilePath}", filePath);
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("RINEX Client - NASA CDDIS Archive Downloader and Parser");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  EphemerisHub [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  download [--date yyyy-MM-dd] [--no-parse]  Download RINEX files");
        Console.WriteLine("  parse <file-path>                          Parse a RINEX file");
        Console.WriteLine("  help                                       Show this help");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --date yyyy-MM-dd    Specify date to download (default: today)");
        Console.WriteLine("  --no-parse           Download only, skip parsing");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  EphemerisHub download");
        Console.WriteLine("  EphemerisHub download --date 2024-01-15");
        Console.WriteLine("  EphemerisHub download --date 2024-01-15 --no-parse");
        Console.WriteLine("  EphemerisHub parse ./downloads/2024/015/BRDM00DLR_S_20240150000_01D_MN.rnx");
        Console.WriteLine();
        Console.WriteLine("To run as a background service (continuous mode), run without arguments:");
        Console.WriteLine("  EphemerisHub");
    }
}
