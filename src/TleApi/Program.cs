using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using TleApi.Configuration;
using TleApi.Data;
using TleApi.DTOs;
using TleApi.Validators;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/tleapi-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Configure TLE options
    var tleOptions = new TleOptions();
    builder.Configuration.GetSection(TleOptions.SectionName).Bind(tleOptions);
    builder.Services.Configure<TleOptions>(builder.Configuration.GetSection(TleOptions.SectionName));

    // Add database context
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' not found.");
    
    builder.Services.AddDbContext<TleDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    }, ServiceLifetime.Scoped);

    // Register repository
    builder.Services.AddScoped<ITleRepository, TleRepository>();

    // Add services to the container
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "TLE API",
            Version = "v1",
            Description = "API for reading Two-Line Element (TLE) data from database",
            Contact = new OpenApiContact
            {
                Name = "TLE API Support"
            }
        });

        // Add XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment() || 
        builder.Configuration.GetValue<bool>("Swagger:Enabled", app.Environment.IsDevelopment()))
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TLE API v1");
            c.RoutePrefix = string.Empty; // Swagger UI at root
        });
    }

    app.UseSerilogRequestLogging();

    // Map endpoints
    MapTleEndpoints(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static void MapTleEndpoints(WebApplication app)
{
    var tleGroup = app.MapGroup("/api/tle")
        .WithTags("TLE")
        .WithOpenApi();

    // GET /api/tle - Get TLE data with filters and pagination
    tleGroup.MapGet("/", async (
        ITleRepository repository,
        string? system,
        string? prn,
        string? datetime,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default) =>
    {
        // Validate system
        if (!TleValidator.IsValidSystem(system, out var systemError))
        {
            return Results.BadRequest(new { error = systemError });
        }

        // Validate prn
        if (!TleValidator.IsValidPrn(prn, out var prnError))
        {
            return Results.BadRequest(new { error = prnError });
        }

        // Validate and parse datetime
        if (!TleValidator.IsValidDateTime(datetime, out var parsedDateTime, out var datetimeError))
        {
            return Results.BadRequest(new { error = datetimeError });
        }

        // Validate pagination
        if (!TleValidator.IsValidPagination(page, pageSize, out var paginationError))
        {
            return Results.BadRequest(new { error = paginationError });
        }

        try
        {
            var result = await repository.GetTlesAsync(system, prn, parsedDateTime, page, pageSize, cancellationToken);

            var response = new PagedResult<TleDto>
            {
                Items = result.Items.Select(MapToDto).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                Total = result.Total
            };

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving TLE data");
            return Results.Problem("An error occurred while retrieving TLE data", statusCode: 500);
        }
    })
    .WithName("GetTles")
    .WithSummary("Get TLE data with optional filtering and pagination")
    .WithDescription("Retrieves TLE (Two-Line Element) data from the database with optional filters for system, PRN, and datetime. Supports pagination.")
    .Produces<PagedResult<TleDto>>(200)
    .Produces<object>(400)
    .Produces(500);

    // GET /api/tle/{system} - Get TLE data for a specific system
    tleGroup.MapGet("/{system}", async (
        string system,
        ITleRepository repository,
        string? datetime,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default) =>
    {
        // Validate system
        if (!TleValidator.IsValidSystem(system, out var systemError))
        {
            return Results.BadRequest(new { error = systemError });
        }

        // Validate and parse datetime
        if (!TleValidator.IsValidDateTime(datetime, out var parsedDateTime, out var datetimeError))
        {
            return Results.BadRequest(new { error = datetimeError });
        }

        // Validate pagination
        if (!TleValidator.IsValidPagination(page, pageSize, out var paginationError))
        {
            return Results.BadRequest(new { error = paginationError });
        }

        try
        {
            var result = await repository.GetTlesAsync(system, null, parsedDateTime, page, pageSize, cancellationToken);

            if (result.Total == 0)
            {
                return Results.NotFound(new { error = $"No TLE data found for system '{system}'" });
            }

            var response = new PagedResult<TleDto>
            {
                Items = result.Items.Select(MapToDto).ToList(),
                Page = result.Page,
                PageSize = result.PageSize,
                Total = result.Total
            };

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving TLE data for system {System}", system);
            return Results.Problem("An error occurred while retrieving TLE data", statusCode: 500);
        }
    })
    .WithName("GetTlesBySystem")
    .WithSummary("Get TLE data for a specific satellite system")
    .WithDescription("Retrieves TLE data for a specific satellite system (GPS, GLONASS, GALILEO, or BEIDOU) with optional datetime filter and pagination.")
    .Produces<PagedResult<TleDto>>(200)
    .Produces<object>(400)
    .Produces<object>(404)
    .Produces(500);

    // GET /api/tle/{system}/{prn} - Get TLE data for a specific satellite
    tleGroup.MapGet("/{system}/{prn}", async (
        string system,
        string prn,
        ITleRepository repository,
        string? datetime,
        CancellationToken cancellationToken = default) =>
    {
        // Validate system
        if (!TleValidator.IsValidSystem(system, out var systemError))
        {
            return Results.BadRequest(new { error = systemError });
        }

        // Validate prn
        if (!TleValidator.IsValidPrn(prn, out var prnError))
        {
            return Results.BadRequest(new { error = prnError });
        }

        // Validate and parse datetime
        if (!TleValidator.IsValidDateTime(datetime, out var parsedDateTime, out var datetimeError))
        {
            return Results.BadRequest(new { error = datetimeError });
        }

        try
        {
            var tle = await repository.GetTleAsync(system, prn, parsedDateTime, cancellationToken);

            if (tle == null)
            {
                return Results.NotFound(new { error = $"No TLE data found for {system}/{prn}" });
            }

            var response = MapToDto(tle);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving TLE data for {System}/{Prn}", system, prn);
            return Results.Problem("An error occurred while retrieving TLE data", statusCode: 500);
        }
    })
    .WithName("GetTleBySystemAndPrn")
    .WithSummary("Get TLE data for a specific satellite")
    .WithDescription("Retrieves the most recent TLE data for a specific satellite identified by system and PRN. Optionally filter by datetime to get the TLE valid at that time.")
    .Produces<TleDto>(200)
    .Produces<object>(400)
    .Produces<object>(404)
    .Produces(500);

    // GET /api/tle/{system}/{prn}/{datetime} - Get TLE data for a specific satellite at a specific time
    tleGroup.MapGet("/{system}/{prn}/{datetime}", async (
        string system,
        string prn,
        string datetime,
        ITleRepository repository,
        CancellationToken cancellationToken = default) =>
    {
        // Validate system
        if (!TleValidator.IsValidSystem(system, out var systemError))
        {
            return Results.BadRequest(new { error = systemError });
        }

        // Validate prn
        if (!TleValidator.IsValidPrn(prn, out var prnError))
        {
            return Results.BadRequest(new { error = prnError });
        }

        // Validate and parse datetime
        if (!TleValidator.IsValidDateTime(datetime, out var parsedDateTime, out var datetimeError))
        {
            return Results.BadRequest(new { error = datetimeError });
        }

        if (!parsedDateTime.HasValue)
        {
            return Results.BadRequest(new { error = "DateTime is required for this endpoint" });
        }

        try
        {
            var tle = await repository.GetTleAsync(system, prn, parsedDateTime.Value, cancellationToken);

            if (tle == null)
            {
                return Results.NotFound(new { error = $"No TLE data found for {system}/{prn} at {datetime}" });
            }

            var response = MapToDto(tle);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving TLE data for {System}/{Prn} at {DateTime}", system, prn, datetime);
            return Results.Problem("An error occurred while retrieving TLE data", statusCode: 500);
        }
    })
    .WithName("GetTleBySystemPrnAndDateTime")
    .WithSummary("Get TLE data for a specific satellite at a specific time")
    .WithDescription("Retrieves TLE data for a specific satellite at a specific datetime. Returns the most recent TLE with epoch <= specified datetime.")
    .Produces<TleDto>(200)
    .Produces<object>(400)
    .Produces<object>(404)
    .Produces(500);
}

static TleDto MapToDto(TleApi.Models.TleEntity entity)
{
    return new TleDto
    {
        System = entity.System,
        Prn = entity.Prn,
        Epoch = entity.Epoch.ToUniversalTime().ToString("O"), // ISO 8601 format
        Line1 = entity.Line1,
        Line2 = entity.Line2
    };
}
