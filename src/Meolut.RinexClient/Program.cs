using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Meolut.Core;
using Meolut.Core.Services;

// Create the host builder
var hostBuilder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true)
              .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
              .AddEnvironmentVariables()
              .AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddRinexServices(context.Configuration);
    })
    .UseConsoleLifetime();

// Build the host
using var host = hostBuilder.Build();

// Initialize database
await host.Services.InitializeDatabaseAsync();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var rinexService = host.Services.GetRequiredService<RinexClientService>();

// Parse command line arguments
var command = args.Length > 0 ? args[0].ToLower() : "help";

try
{
    switch (command)
    {
        case "download":
            await HandleDownloadCommand(args, rinexService, logger);
            break;
            
        case "status":
            await HandleStatusCommand(args, rinexService);
            break;
            
        case "cleanup":
            await HandleCleanupCommand(args, rinexService, logger);
            break;
            
        case "help":
        case "--help":
        case "-h":
        default:
            ShowHelp();
            break;
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Application error");
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}

return 0;

static async Task HandleDownloadCommand(string[] args, RinexClientService rinexService, ILogger logger)
{
    DateOnly? specificDate = null;
    DateOnly? startDate = null;
    DateOnly? endDate = null;
    bool force = false;

    // Parse arguments
    for (int i = 1; i < args.Length; i++)
    {
        switch (args[i].ToLower())
        {
            case "--date":
                if (i + 1 < args.Length && DateOnly.TryParse(args[i + 1], out var date))
                {
                    specificDate = date;
                    i++; // Skip next argument
                }
                break;
                
            case "--start-date":
                if (i + 1 < args.Length && DateOnly.TryParse(args[i + 1], out var start))
                {
                    startDate = start;
                    i++; // Skip next argument
                }
                break;
                
            case "--end-date":
                if (i + 1 < args.Length && DateOnly.TryParse(args[i + 1], out var end))
                {
                    endDate = end;
                    i++; // Skip next argument
                }
                break;
                
            case "--force":
                force = true;
                break;
        }
    }

    if (specificDate.HasValue)
    {
        // Download single date
        logger.LogInformation("Processing RINEX file for date: {Date}", specificDate.Value);
        var result = await rinexService.ProcessDateAsync(specificDate.Value, force);
        PrintResult(result);
    }
    else if (startDate.HasValue && endDate.HasValue)
    {
        // Download date range
        logger.LogInformation("Processing RINEX files from {StartDate} to {EndDate}", startDate.Value, endDate.Value);
        var results = await rinexService.ProcessDateRangeAsync(startDate.Value, endDate.Value, force);
        PrintResults(results);
    }
    else if (startDate.HasValue)
    {
        // Download from start date to today
        var today = DateOnly.FromDateTime(DateTime.Today);
        logger.LogInformation("Processing RINEX files from {StartDate} to {EndDate}", startDate.Value, today);
        var results = await rinexService.ProcessDateRangeAsync(startDate.Value, today, force);
        PrintResults(results);
    }
    else
    {
        // Download today's file
        var today = DateOnly.FromDateTime(DateTime.Today);
        logger.LogInformation("Processing RINEX file for today: {Date}", today);
        var result = await rinexService.ProcessDateAsync(today, force);
        PrintResult(result);
    }
}

static async Task HandleStatusCommand(string[] args, RinexClientService rinexService)
{
    DateOnly? startDate = null;
    DateOnly? endDate = null;
    int days = 7;

    // Parse arguments
    for (int i = 1; i < args.Length; i++)
    {
        switch (args[i].ToLower())
        {
            case "--start-date":
                if (i + 1 < args.Length && DateOnly.TryParse(args[i + 1], out var startParsed))
                {
                    startDate = startParsed;
                    i++; // Skip next argument
                }
                break;
                
            case "--end-date":
                if (i + 1 < args.Length && DateOnly.TryParse(args[i + 1], out var endParsed))
                {
                    endDate = endParsed;
                    i++; // Skip next argument
                }
                break;
                
            case "--days":
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out var d))
                {
                    days = d;
                    i++; // Skip next argument
                }
                break;
        }
    }

    DateOnly start, end;
    
    if (startDate.HasValue && endDate.HasValue)
    {
        start = startDate.Value;
        end = endDate.Value;
    }
    else if (startDate.HasValue)
    {
        start = startDate.Value;
        end = start.AddDays(days - 1);
    }
    else
    {
        end = DateOnly.FromDateTime(DateTime.Today);
        start = end.AddDays(-(days - 1));
    }
    
    Console.WriteLine($"Data status from {start} to {end}:");
    Console.WriteLine();
    
    var statusList = await rinexService.GetDataStatusAsync(start, end);
    
    Console.WriteLine("Date       | Has Data | File Available");
    Console.WriteLine("-----------|----------|---------------");
    
    foreach (var status in statusList)
    {
        var hasDataIcon = status.HasData ? "✓" : "✗";
        var fileAvailableIcon = status.FileAvailable ? "✓" : "✗";
        Console.WriteLine($"{status.Date:yyyy-MM-dd} | {hasDataIcon,8} | {fileAvailableIcon,13}");
    }
    
    var totalDays = statusList.Count;
    var daysWithData = statusList.Count(s => s.HasData);
    var daysWithFiles = statusList.Count(s => s.FileAvailable);
    
    Console.WriteLine();
    Console.WriteLine($"Summary: {daysWithData}/{totalDays} days have data, {daysWithFiles}/{totalDays} days have files available");
}

static async Task HandleCleanupCommand(string[] args, RinexClientService rinexService, ILogger logger)
{
    int days = 30;
    bool confirm = false;

    // Parse arguments
    for (int i = 1; i < args.Length; i++)
    {
        switch (args[i].ToLower())
        {
            case "--days":
                if (i + 1 < args.Length && int.TryParse(args[i + 1], out var d))
                {
                    days = d;
                    i++; // Skip next argument
                }
                break;
                
            case "--confirm":
                confirm = true;
                break;
        }
    }

    var cutoffDate = DateTime.UtcNow.AddDays(-days);
    Console.WriteLine($"This will delete all navigation data older than {cutoffDate:yyyy-MM-dd HH:mm:ss} UTC");
    
    if (!confirm)
    {
        Console.Write("Are you sure? (y/N): ");
        var response = Console.ReadLine();
        if (response?.ToLower() != "y" && response?.ToLower() != "yes")
        {
            Console.WriteLine("Operation cancelled");
            return;
        }
    }
    
    var deletedCount = await rinexService.CleanupOldDataAsync(days);
    Console.WriteLine($"Successfully deleted {deletedCount} old records");
}

static void ShowHelp()
{
    Console.WriteLine("RINEX Navigation Data Client");
    Console.WriteLine("Downloads and processes GNSS navigation data from NASA CDDIS archive");
    Console.WriteLine();
    Console.WriteLine("Usage: Meolut.RinexClient [command] [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  download    Download and process RINEX navigation files");
    Console.WriteLine("  status      Check data availability status");
    Console.WriteLine("  cleanup     Clean up old navigation data");
    Console.WriteLine("  help        Show this help message");
    Console.WriteLine();
    Console.WriteLine("Download options:");
    Console.WriteLine("  --date <date>        Specific date to download (YYYY-MM-DD)");
    Console.WriteLine("  --start-date <date>  Start date for range download (YYYY-MM-DD)");
    Console.WriteLine("  --end-date <date>    End date for range download (YYYY-MM-DD)");
    Console.WriteLine("  --force              Force download even if data already exists");
    Console.WriteLine();
    Console.WriteLine("Status options:");
    Console.WriteLine("  --start-date <date>  Start date for status check (YYYY-MM-DD)");
    Console.WriteLine("  --end-date <date>    End date for status check (YYYY-MM-DD)");
    Console.WriteLine("  --days <number>      Number of days to check (default: 7)");
    Console.WriteLine();
    Console.WriteLine("Cleanup options:");
    Console.WriteLine("  --days <number>      Keep data for this many days (default: 30)");
    Console.WriteLine("  --confirm            Confirm deletion without prompting");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  Meolut.RinexClient download --date 2025-07-03");
    Console.WriteLine("  Meolut.RinexClient download --start-date 2025-07-01 --end-date 2025-07-03");
    Console.WriteLine("  Meolut.RinexClient status --days 14");
    Console.WriteLine("  Meolut.RinexClient cleanup --days 60 --confirm");
}

static void PrintResult(RinexProcessingResult result)
{
    var statusIcon = result.Status switch
    {
        ProcessingStatus.Success => "✓",
        ProcessingStatus.Skipped => "⊘",
        ProcessingStatus.FileNotAvailable => "⚠",
        ProcessingStatus.NoData => "?",
        ProcessingStatus.Error => "✗",
        _ => "?"
    };
    
    Console.WriteLine($"{statusIcon} {result.Date:yyyy-MM-dd} - {result.Status}: {result.Message}");
    
    if (result.Status == ProcessingStatus.Success && result.ParseResult != null)
    {
        var parseResult = result.ParseResult;
        Console.WriteLine($"   GPS: {parseResult.GpsData.Count}, GLONASS: {parseResult.GlonassData.Count}, " +
                         $"Galileo: {parseResult.GalileoData.Count}, BeiDou: {parseResult.BeidouData.Count}");
    }
}

static void PrintResults(List<RinexProcessingResult> results)
{
    foreach (var result in results)
    {
        PrintResult(result);
    }
    
    Console.WriteLine();
    var successCount = results.Count(r => r.Status == ProcessingStatus.Success);
    var totalRecords = results.Sum(r => r.RecordsSaved);
    Console.WriteLine($"Summary: {successCount}/{results.Count} dates processed successfully, {totalRecords} total records saved");
}
