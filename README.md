# EphemerisHub - GNSS TLE Service

A .NET 9 service for managing GNSS (Global Navigation Satellite System) satellite orbital data using TLE (Two-Line Element) format.

## Overview

This service provides automatic downloading, storage, and REST API access to TLE orbital data for GPS, GLONASS, Galileo, and BeiDou satellite systems.

### What is TLE?

TLE (Two-Line Element set) is a standard format for describing satellite orbits. It contains:
- Basic orbital elements (semi-major axis, eccentricity, inclination, etc.)
- Time of epoch
- Mean motion and anomaly
- NORAD catalog number

Unlike full ephemerides (RINEX/SP3 format), TLE is:
- Much more compact (2-3 lines per satellite)
- Updated less frequently
- Suitable for satellite tracking and basic orbital calculations
- Less precise but sufficient for many applications

## Features

### REST API Endpoints

- `GET /api/tle` - Get all TLE data
- `GET /api/tle/{system}` - Get TLE data by GNSS system (GPS, GLONASS, Galileo, BeiDou)
- `GET /api/tle/{system}/{satnum}` - Get TLE for specific satellite number
- `GET /api/satellites` - List all active satellites
- `GET /api/systems` - List supported GNSS systems with statistics
- `GET /api/status` - Service status and system statistics
- `GET /health` - Health check endpoint

### Background Services

1. **TLE Download Service**
   - Automatically downloads TLE data from configured sources
   - Default interval: 15 minutes (configurable)
   - Resilient with retry logic and circuit breaker

2. **Data Cleanup Service**
   - Automatically removes old TLE data
   - Default retention: 730 days (2 years)
   - Runs daily at configured hour

## Technology Stack

- **.NET 9** with Minimal API
- **PostgreSQL** for data storage
- **Entity Framework Core** for ORM
- **Polly** for resilience (retry, circuit breaker)
- **Serilog** for structured logging
- **Health Checks** for monitoring

## Getting Started

### Prerequisites

- .NET 9 SDK
- PostgreSQL 16+
- Docker (optional, for local development)

### Local Development with Docker

1. Start PostgreSQL:
   ```bash
   docker compose up -d
   ```

2. Apply database migrations:
   ```bash
   cd src/EphemerisHub
   dotnet ef database update
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5101`

### Configuration

Configuration is done through `appsettings.json` and `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=ephemerishub;Username=postgres;Password=postgres"
  },
  "TleLoader": {
    "ExecuteInterval": "00:15:00"
  },
  "Tle": {
    "Uris": [
      "https://celestrak.org/NORAD/elements/gp.php?GROUP=gnss&FORMAT=tle"
    ]
  },
  "DataCleanup": {
    "RetentionDays": 730,
    "CleanupHour": 1
  },
  "MeosarSatellites": {
    "GpsNoradIds": { "40105": 305, "40534": 306 },
    "GalileoNoradIds": { "37846": 401, "37847": 402 },
    "GlonassNoradIds": { "40258": 501 },
    "BeiDouNoradIds": { "43001": 601 }
  }
}
```

#### NORAD ID Mapping

The `MeosarSatellites` section maps NORAD catalog IDs to CS satellite numbers for MEOSAR tracking. Configure this based on your specific requirements.

## Database Schema

The service uses separate tables for each GNSS system:
- `GpsTle` - GPS satellites
- `GalileoTle` - Galileo satellites
- `GlonassTle` - GLONASS satellites
- `BeidouTle` - BeiDou satellites

Each table has a composite primary key of `(CsSatNum, Time)` to store historical TLE data.

## API Examples

### Get all systems with counts
```bash
curl http://localhost:5101/api/systems
```

Response:
```json
[
  {
    "name": "GPS",
    "description": "Global Positioning System (USA)",
    "satelliteCount": 16
  },
  {
    "name": "GLONASS",
    "description": "Global Navigation Satellite System (Russia)",
    "satelliteCount": 4
  }
]
```

### Get GPS satellite TLE
```bash
curl http://localhost:5101/api/tle/GPS/305
```

Response:
```json
{
  "name": "GPS BIIA-11 (PRN 20)",
  "line1": "1 20959U 90103A   25310.00000000  .00000000  00000-0  00000-0 0  9999",
  "line2": "2 20959  54.9999 123.4567  0.0001234  12.3456  78.9012  2.00000000999999",
  "csSatNum": 305,
  "time": "2025-11-06T07:06:12.375155Z",
  "system": "GPS"
}
```

### Get service status
```bash
curl http://localhost:5101/api/status
```

Response:
```json
{
  "status": "Running",
  "lastDownload": "2025-11-06T07:06:12.377856Z",
  "systems": {
    "GPS": {
      "recordCount": 2,
      "lastUpdate": "2025-11-06T07:06:12.375155Z"
    },
    "Galileo": {
      "recordCount": 2,
      "lastUpdate": "2025-11-06T07:06:12.376562Z"
    }
  }
}
```

## Resilience & Monitoring

### Retry & Circuit Breaker

The service uses Polly policies for HTTP resilience:
- **Retry Policy**: 3 retries with exponential backoff
- **Circuit Breaker**: Opens after 5 consecutive failures, stays open for 30 seconds

### Logging

Structured logging with Serilog:
- Console output for development
- File logging (rolling daily) in `logs/` directory
- JSON format for production integration

### Health Checks

The `/health` endpoint provides:
- Database connectivity check
- Overall service health status

## Architecture

```
EphemerisHub/
├── Adapters/              # Infrastructure adapters (HTTP, DB)
│   ├── TleAdapter.cs      # HTTP client for TLE downloads
│   └── TleRepository.cs   # Database repository
├── Application/           # Application layer
│   ├── ITleAdapter.cs
│   ├── ITleRepository.cs
│   ├── TleDownloadService.cs  # Background download service
│   └── TleParser.cs       # TLE format parser
├── DTOs/                  # API response models
├── Endpoints/             # Minimal API endpoints
├── Infrastructure/        # Cross-cutting concerns
│   ├── Configuration/
│   ├── Database/
│   ├── Settings/
│   └── PollyPolicies.cs
├── Models/                # Domain models
├── Services/              # Business logic
│   ├── DataCleanupService.cs
│   └── TleService.cs
└── Program.cs             # Application entry point
```

## Development

### Adding a New GNSS System

1. Create a new model inheriting from `Tle`
2. Add DbSet to `AppDbContext`
3. Create entity configuration
4. Update `TleExtensions.MapByCsSatNum` for CS number mapping
5. Update repository methods to include the new system
6. Create and apply migration

### Running Tests

```bash
dotnet test
```

## Production Deployment

For production deployment:

1. Set up a PostgreSQL database
2. Configure connection string and settings
3. Run migrations: `dotnet ef database update`
4. Deploy the application
5. Configure TLE source URLs if different from default
6. Set up monitoring for health checks
7. Configure log aggregation

## License

See LICENSE file for details.

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing patterns
- All tests pass
- Security scans are clean
- Documentation is updated
