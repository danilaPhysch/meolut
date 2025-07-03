using EphemerisHub.Infrastructure.Configuration;
using EphemerisHub.Infrastructure.Database;
using EphemerisHub.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Globalization;

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

            // Handle command line arguments
            if (args.Length > 0)
            {
                await HandleCommandLineAsync(host, args);
            }
            else
            {
                // Initialize database only for background service mode
                await host.SeedDbContext();
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
        var command = args[0].ToLower();

        // For help and demo commands, we don't need database
        if (command is "help" or "--help" or "-h")
        {
            ShowHelp();
            return;
        }

        if (command == "demo")
        {
            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            await HandleDemoCommand(logger, args);
            return;
        }

        // Initialize database for other commands
        await host.SeedDbContext();

        using var scope2 = host.Services.CreateScope();
        var downloader = scope2.ServiceProvider.GetRequiredService<IRinexDownloader>();
        var parser = scope2.ServiceProvider.GetRequiredService<IRinexParser>();
        var logger2 = scope2.ServiceProvider.GetRequiredService<ILogger<Program>>();

        switch (command)
        {
            case "download":
                await HandleDownloadCommand(downloader, parser, logger2, args);
                break;
            case "parse":
                await HandleParseCommand(parser, logger2, args);
                break;
            default:
                logger2.LogError("Unknown command: {Command}. Use 'help' for available commands.", command);
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

    private static async Task HandleDemoCommand(ILogger<Program> logger, string[] args)
    {
        var sampleFilePath = Path.Combine("sample_data", "BRDM00DLR_S_20250150000_01D_MN.rnx");
        
        if (!File.Exists(sampleFilePath))
        {
            logger.LogError("Sample RINEX file not found: {FilePath}", sampleFilePath);
            return;
        }

        try
        {
            logger.LogInformation("=== RINEX Client Demo Mode ===");
            logger.LogInformation("Analyzing sample RINEX file: {FilePath}", sampleFilePath);
            
            var lines = await File.ReadAllLinesAsync(sampleFilePath);
            logger.LogInformation("File contains {LineCount} lines", lines.Length);
            
            // Parse header
            var headerInfo = await ParseRinexHeaderDemo(lines);
            logger.LogInformation("RINEX Version: {Version}", headerInfo.Version);
            logger.LogInformation("File Type: {FileType}", headerInfo.FileType);
            
            // Find ephemeris records
            var ephemerisRecords = await FindEphemerisRecordsDemo(lines);
            logger.LogInformation("Found {Count} ephemeris records:", ephemerisRecords.Count);
            
            foreach (var record in ephemerisRecords)
            {
                logger.LogInformation("  - {System}{PRN:D2} at {Time}", record.System, record.PRN, record.Time.ToString("yyyy-MM-dd HH:mm:ss UTC"));
            }
            
            // Show satellite systems
            var systems = ephemerisRecords.GroupBy(r => r.System).ToDictionary(g => g.Key, g => g.Count());
            logger.LogInformation("Satellite systems found:");
            foreach (var system in systems)
            {
                var systemName = system.Key switch
                {
                    'G' => "GPS",
                    'R' => "GLONASS", 
                    'E' => "Galileo",
                    'C' => "BeiDou",
                    _ => "Unknown"
                };
                logger.LogInformation("  - {SystemName} ({System}): {Count} satellites", systemName, system.Key, system.Value);
            }
            
            logger.LogInformation("=== Demo completed successfully ===");
            logger.LogInformation("In production mode, this data would be saved to the database.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Demo failed: {Message}", ex.Message);
        }
    }

    private static async Task<(double Version, string FileType)> ParseRinexHeaderDemo(string[] lines)
    {
        foreach (var line in lines)
        {
            if (line.Contains("RINEX VERSION / TYPE"))
            {
                var version = double.Parse(line.Substring(0, 9).Trim(), CultureInfo.InvariantCulture);
                var fileType = line.Substring(20, 20).Trim();
                return (version, fileType);
            }
            if (line.Contains("END OF HEADER"))
                break;
        }
        return (0.0, "UNKNOWN");
    }

    private static async Task<List<(char System, int PRN, DateTime Time)>> FindEphemerisRecordsDemo(string[] lines)
    {
        var records = new List<(char System, int PRN, DateTime Time)>();
        bool headerEnded = false;
        
        foreach (var line in lines)
        {
            if (line.Contains("END OF HEADER"))
            {
                headerEnded = true;
                continue;
            }
            
            if (!headerEnded || line.Length < 3)
                continue;
                
            var satelliteSystem = line[0];
            if (satelliteSystem is 'G' or 'R' or 'E' or 'C')
            {
                try
                {
                    var prnStr = line.Substring(1, 2).Trim();
                    if (int.TryParse(prnStr, out var prn))
                    {
                        var year = int.Parse(line.Substring(4, 4));
                        var month = int.Parse(line.Substring(9, 2));
                        var day = int.Parse(line.Substring(12, 2));
                        var hour = int.Parse(line.Substring(15, 2));
                        var minute = int.Parse(line.Substring(18, 2));
                        var second = int.Parse(line.Substring(21, 2));
                        
                        var time = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
                        records.Add((satelliteSystem, prn, time));
                    }
                }
                catch
                {
                    // Skip invalid lines
                }
            }
        }
        
        return records;
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
        Console.WriteLine("  demo                                       Run demo with sample data");
        Console.WriteLine("  help                                       Show this help");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --date yyyy-MM-dd    Specify date to download (default: today)");
        Console.WriteLine("  --no-parse           Download only, skip parsing");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  EphemerisHub demo");
        Console.WriteLine("  EphemerisHub download");
        Console.WriteLine("  EphemerisHub download --date 2024-01-15");
        Console.WriteLine("  EphemerisHub download --date 2024-01-15 --no-parse");
        Console.WriteLine("  EphemerisHub parse ./downloads/2024/015/BRDM00DLR_S_20240150000_01D_MN.rnx");
        Console.WriteLine();
        Console.WriteLine("To run as a background service (continuous mode), run without arguments:");
        Console.WriteLine("  EphemerisHub");
    }
}
