# MEOLUT - RINEX Navigation Data Client

A .NET 8 client application for downloading, parsing, and storing GNSS navigation data from NASA's CDDIS (Crustal Dynamics Data Information System) archive.

## Features

- **Multi-GNSS Support**: Supports GPS, GLONASS, Galileo, and BeiDou navigation systems
- **Automatic Download**: Downloads RINEX navigation files from NASA CDDIS archive
- **RINEX Parser**: Comprehensive parser for RINEX 3.x navigation data format
- **Database Storage**: SQLite database with optimized schema for navigation data
- **Data Validation**: Validates navigation data before storage
- **CLI Interface**: Command-line interface for easy operation
- **Error Handling**: Robust error handling and logging
- **Incremental Updates**: Avoids duplicate downloads and supports data updates

## Architecture

### Core Components

- **Meolut.Core**: Core library containing all business logic
  - `RinexParser`: Parses RINEX navigation files
  - `RinexDownloadService`: Downloads files from NASA CDDIS
  - `RinexDataService`: Manages database operations
  - `RinexClientService`: Main orchestration service
  
- **Meolut.RinexClient**: Console application with CLI interface

### Database Schema

The application uses SQLite with separate tables for each GNSS system:
- `GpsNavigationData`: GPS navigation parameters
- `GlonassNavigationData`: GLONASS state vectors and parameters
- `GalileoNavigationData`: Galileo navigation parameters
- `BeidouNavigationData`: BeiDou navigation parameters

Each table includes optimized indexes for fast queries by satellite and time.

## Installation

### Prerequisites

- .NET 8.0 SDK
- Internet connection for downloading RINEX files

### Build

```bash
git clone https://github.com/danilaPhysch/meolut.git
cd meolut
dotnet build
```

## Usage

### Download Today's RINEX File

```bash
dotnet run --project src/Meolut.RinexClient download
```

### Download Specific Date

```bash
dotnet run --project src/Meolut.RinexClient download --date 2025-07-03
```

### Download Date Range

```bash
dotnet run --project src/Meolut.RinexClient download --start-date 2025-07-01 --end-date 2025-07-03
```

### Check Data Status

```bash
# Check last 7 days
dotnet run --project src/Meolut.RinexClient status

# Check specific date range
dotnet run --project src/Meolut.RinexClient status --start-date 2025-07-01 --end-date 2025-07-03

# Check last 30 days
dotnet run --project src/Meolut.RinexClient status --days 30
```

### Clean Up Old Data

```bash
# Keep last 30 days (default)
dotnet run --project src/Meolut.RinexClient cleanup

# Keep last 60 days
dotnet run --project src/Meolut.RinexClient cleanup --days 60

# Auto-confirm deletion
dotnet run --project src/Meolut.RinexClient cleanup --days 90 --confirm
```

### Force Update

```bash
# Force download even if data already exists
dotnet run --project src/Meolut.RinexClient download --date 2025-07-03 --force
```

## Configuration

Configuration is handled through `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=rinex.db"
  },
  "RinexDownload": {
    "BaseUrl": "https://cddis.nasa.gov/archive/gnss/data/daily",
    "TimeoutSeconds": 120,
    "MaxRetryAttempts": 3,
    "RetryDelayMs": 1000
  }
}
```

## RINEX File Format

The client processes RINEX navigation files with the naming convention:
`BRDM00DLR_S_YYYYDDD0000_01D_MN.rnx.gz`

Where:
- `YYYY`: Year
- `DDD`: Day of year (001-366)

Files are automatically decompressed from .gz format.

## Navigation Data

### GPS Navigation Data
Standard broadcast ephemeris parameters including:
- Orbital elements (semi-major axis, eccentricity, inclination)
- Perturbation corrections (Cuc, Cus, Crc, Crs, Cic, Cis)
- Clock correction parameters
- Time references and health status

### GLONASS Navigation Data
State vectors including:
- Position coordinates (X, Y, Z)
- Velocity components
- Acceleration components
- Frequency number and health status

### Galileo Navigation Data
Similar to GPS with Galileo-specific parameters:
- IOD navigation data
- Signal-in-Space Accuracy (SISA)
- Background group delays

### BeiDou Navigation Data
BeiDou-specific broadcast parameters:
- Age of Data Ephemeris (AODE)
- Total Group Delays (TGD1, TGD2)
- Age of Data Clock (AODC)

## Error Handling

The application includes comprehensive error handling:
- Network timeouts and retries
- File format validation
- Database constraint handling
- Graceful degradation for missing data

## Logging

Structured logging is implemented using Microsoft.Extensions.Logging:
- Information level for normal operations
- Warning level for recoverable errors
- Error level for critical failures

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- NASA CDDIS for providing RINEX navigation data
- IGS (International GNSS Service) for RINEX format standards
- RINEX Working Group for format documentation

## References

- [RINEX 3.05 Format Specification](https://files.igs.org/pub/data/format/rinex305.pdf)
- [NASA CDDIS Archive](https://cddis.nasa.gov/archive/gnss/data/daily/)
- [IGS Products](https://www.igs.org/products/)