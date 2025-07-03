# RINEX Client for NASA CDDIS Archive

A .NET console application for downloading and parsing RINEX navigation files from NASA's Crustal Dynamics Data Information System (CDDIS) archive.

## Features

- ✅ Download RINEX files from NASA CDDIS archive
- ✅ Parse RINEX navigation files (version 2.x and 3.x)
- ✅ Support for multiple GNSS systems:
  - GPS (G)
  - GLONASS (R)
  - Galileo (E)
  - BeiDou (C)
- ✅ Save ephemeris data to PostgreSQL database
- ✅ Automatic scheduling for daily downloads
- ✅ Command-line interface for manual operations
- ✅ Comprehensive logging with Serilog
- ✅ Error handling and retry mechanisms
- ✅ Duplicate prevention in database

## Quick Start

### 1. Demo Mode (No Database Required)

Test the RINEX parsing functionality with sample data:

```bash
dotnet run demo
```

This will analyze a sample RINEX file and show what would be parsed.

### 2. View Help

```bash
dotnet run help
```

## Project Structure

```
src/EphemerisHub/
├── Program.cs                              # Console application entry point
├── appsettings.json                        # Configuration file
├── Models/                                 # Data models
│   ├── RinexEphemeris.cs                   # Base ephemeris model
│   ├── RinexGpsEphemeris.cs               # GPS-specific ephemeris
│   ├── RinexGlonassEphemeris.cs           # GLONASS-specific ephemeris
│   ├── RinexGalileoEphemeris.cs           # Galileo-specific ephemeris
│   └── RinexBeidouEphemeris.cs            # BeiDou-specific ephemeris
├── Services/                               # Business logic
│   ├── RinexDownloader.cs                 # Download service
│   ├── RinexParser.cs                     # RINEX file parser
│   └── RinexSchedulerService.cs           # Background scheduling service
├── Infrastructure/                         # Infrastructure layer
│   ├── Configuration/                     # Configuration management
│   │   ├── AppConfiguration.cs
│   │   └── RinexConfiguration.cs
│   └── Database/                          # Data access layer
│       ├── AppDbContext.cs
│       ├── AppDbContextSeed.cs
│       └── EntityConfiguration/           # EF Core configurations
└── sample_data/                           # Sample RINEX files for demo
```

## Commands

### Demo Mode
```bash
# Run demo with sample data (no database required)
dotnet run demo
```

### Download Files
```bash
# Download today's RINEX files and parse them
dotnet run download

# Download files for a specific date
dotnet run download --date 2024-01-15

# Download only (skip parsing)
dotnet run download --date 2024-01-15 --no-parse
```

### Parse Files
```bash
# Parse a specific RINEX file
dotnet run parse ./downloads/2024/015/BRDM00DLR_S_20240150000_01D_MN.rnx
```

### Background Service
```bash
# Run as continuous background service
dotnet run
```

## Configuration

Configure the application via `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=ephemerishub;Username=username;Password=password"
  },
  "Rinex": {
    "BaseUrl": "https://cddis.nasa.gov/archive/gnss/data/daily/",
    "DownloadDirectory": "./downloads",
    "ScheduleInterval": "24:00:00",
    "MaxRetryAttempts": 3,
    "RetryDelay": "00:05:00",
    "AutoDownload": true,
    "DaysToDownload": 1
  }
}
```

### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `BaseUrl` | NASA CDDIS archive base URL | `https://cddis.nasa.gov/archive/gnss/data/daily/` |
| `DownloadDirectory` | Local directory for downloaded files | `./downloads` |
| `ScheduleInterval` | Interval between automatic downloads | `24:00:00` (24 hours) |
| `MaxRetryAttempts` | Maximum retry attempts for failed downloads | `3` |
| `RetryDelay` | Delay between retry attempts | `00:05:00` (5 minutes) |
| `AutoDownload` | Enable automatic downloading | `true` |
| `DaysToDownload` | Number of recent days to download | `1` |

## Database Setup

1. Install PostgreSQL
2. Create a database named `ephemerishub`
3. Update the connection string in `appsettings.json`
4. Run the application - Entity Framework will create tables automatically

### Database Schema

The application creates tables for each GNSS system:
- `GpsEphemeris` - GPS satellite ephemeris data
- `GlonassEphemeris` - GLONASS satellite ephemeris data
- `GalileoEphemeris` - Galileo satellite ephemeris data
- `BeidouEphemeris` - BeiDou satellite ephemeris data

Each table includes:
- Satellite identification (system, PRN)
- Time of clock reference
- Clock coefficients (bias, drift, drift rate)
- Orbital parameters (system-specific)

## RINEX File Format Support

### Supported RINEX Versions
- RINEX 2.x navigation files
- RINEX 3.x navigation files (mixed GNSS)

### Supported File Types
- Navigation data files (`.rnx`, `.nav`)
- Compressed files (`.gz`)

### File Naming Convention
The application downloads files following the IGS naming convention:
```
BRDM00DLR_S_YYYYDDDS_01D_MN.rnx.gz
```
Where:
- `YYYY` = Year
- `DDD` = Day of year (001-366)
- `S` = Session (0 for daily files)

## Architecture

### Layered Architecture
1. **Presentation Layer**: Console interface (`Program.cs`)
2. **Service Layer**: Business logic (`Services/`)
3. **Infrastructure Layer**: Data access and external services (`Infrastructure/`)
4. **Domain Layer**: Data models (`Models/`)

### Key Components

#### RinexDownloader
- Downloads files from NASA CDDIS archive
- Handles HTTP requests with retry logic
- Automatically extracts compressed files
- Prevents duplicate downloads

#### RinexParser
- Parses RINEX navigation files
- Supports multiple GNSS systems
- Extracts ephemeris parameters
- Validates data integrity

#### RinexSchedulerService
- Background service for automatic downloads
- Configurable scheduling intervals
- Handles errors gracefully
- Prevents duplicate processing

## Error Handling

The application includes comprehensive error handling:
- Network timeouts and connection issues
- File parsing errors
- Database connection problems
- Invalid RINEX file formats
- Missing files or directories

## Logging

Structured logging using Serilog:
- Console output for interactive use
- File logging for background service
- Configurable log levels
- Detailed error information

Log files are stored in `logs/` directory with daily rotation.

## Development

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL database
- Internet connection (for downloading RINEX files)

### Building
```bash
dotnet build
```

### Running Tests
```bash
# Run demo to test parsing functionality
dotnet run demo

# Test help output
dotnet run help
```

### Adding New GNSS Systems

1. Create a new model class inheriting from `RinexEphemeris`
2. Add Entity Framework configuration
3. Update `AppDbContext` with new DbSet
4. Implement parsing logic in `RinexParser`
5. Add migration for database schema

## Production Deployment

1. Configure database connection string
2. Set appropriate log levels
3. Configure automatic downloads
4. Set up monitoring and alerting
5. Consider running as a system service

### Running as System Service

On Linux with systemd:
```bash
# Create service file
sudo nano /etc/systemd/system/rinex-client.service

# Enable and start service
sudo systemctl enable rinex-client
sudo systemctl start rinex-client
```

## Troubleshooting

### Common Issues

1. **Database Connection Errors**
   - Verify PostgreSQL is running
   - Check connection string format
   - Ensure database exists

2. **Download Failures**
   - Check internet connectivity
   - Verify NASA CDDIS archive availability
   - Review retry configuration

3. **Parsing Errors**
   - Validate RINEX file format
   - Check for corrupted downloads
   - Review log files for details

### Support

For issues and questions:
1. Check log files in `logs/` directory
2. Run demo mode to verify basic functionality
3. Review configuration settings
4. Check database connectivity

## License

MIT License - see LICENSE file for details.