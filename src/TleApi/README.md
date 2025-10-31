# TLE API

API for reading Two-Line Element (TLE) data from PostgreSQL database without external API calls.

## Overview

The TLE API provides RESTful endpoints to query TLE (Two-Line Element) data for GNSS satellites stored in a PostgreSQL database. It supports filtering by satellite system, PRN, and datetime, with pagination support.

## Features

- **Read-only API** - No external API calls, reads directly from PostgreSQL database
- **Filtering** - Filter by satellite system (GPS, GLONASS, GALILEO, BEIDOU), PRN, and datetime
- **Pagination** - Configurable pagination (1-200 items per page)
- **OpenAPI/Swagger** - Full API documentation with Swagger UI
- **Validation** - Comprehensive input validation with clear error messages
- **Logging** - Structured logging with Serilog

## Supported Satellite Systems

- `GPS` - Global Positioning System
- `GLONASS` - Russian GLONASS
- `GALILEO` - European Galileo
- `BEIDOU` - Chinese BeiDou

## Prerequisites

- .NET 9.0 SDK
- PostgreSQL database with TLE data

## Configuration

### Connection String

Configure the database connection in `appsettings.json` or via environment variables:

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=meolut;Username=postgres;Password=postgres"
  }
}
```

**Environment Variables:**
```bash
export ConnectionStrings__Default="Host=localhost;Database=meolut;Username=postgres;Password=postgres"
```

### Table Configuration

By default, the API expects TLE data in a table named `tle`. You can customize this:

**appsettings.json:**
```json
{
  "Tle": {
    "TableName": "tle"
  }
}
```

**Environment Variables:**
```bash
export Tle__TableName="custom_tle_table"
```

### Database Schema

The API expects the following columns in the TLE table:

| Column | Type | Description |
|--------|------|-------------|
| `system` | text/varchar | Satellite system (GPS, GLONASS, GALILEO, BEIDOU) |
| `prn` | text/varchar | PRN identifier (e.g., G01, R12, E24, C03) |
| `epoch` | timestamp with time zone | TLE epoch/validity time |
| `line1` | text | TLE line 1 |
| `line2` | text | TLE line 2 |

## Running the API

### Local Development

```bash
cd src/TleApi
dotnet run
```

The API will start on `https://localhost:5001` (or the port configured in `launchSettings.json`).

### Production

```bash
cd src/TleApi
dotnet publish -c Release -o publish
cd publish
./TleApi
```

### Docker (Optional)

```bash
docker build -t tleapi .
docker run -p 5000:8080 -e ConnectionStrings__Default="Host=host.docker.internal;Database=meolut;Username=postgres;Password=postgres" tleapi
```

## API Endpoints

### Swagger UI

Open your browser and navigate to:
- Development: `http://localhost:5000` or `https://localhost:5001`
- The Swagger UI will be displayed at the root URL

### Endpoints

#### 1. GET /api/tle

Get TLE data with optional filtering and pagination.

**Query Parameters:**
- `system` (optional) - Satellite system (GPS, GLONASS, GALILEO, BEIDOU)
- `prn` (optional) - PRN identifier
- `datetime` (optional) - ISO 8601 datetime (returns records with epoch <= datetime)
- `page` (optional, default: 1) - Page number (>= 1)
- `pageSize` (optional, default: 50) - Items per page (1-200)

**Example:**
```bash
curl "http://localhost:5000/api/tle?system=GPS&page=1&pageSize=10"
```

**Response:**
```json
{
  "items": [
    {
      "system": "GPS",
      "prn": "G01",
      "epoch": "2024-10-30T12:00:00.0000000Z",
      "line1": "1 37753U 11036A   24304.50000000  .00000012  00000-0  00000+0 0  9993",
      "line2": "2 37753  55.0000 195.0000 0000001  30.0000 330.0000  2.00000000 12345"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "total": 32
}
```

#### 2. GET /api/tle/{system}

Get TLE data for a specific satellite system.

**Path Parameters:**
- `system` - Satellite system (GPS, GLONASS, GALILEO, BEIDOU)

**Query Parameters:**
- `datetime` (optional) - ISO 8601 datetime
- `page` (optional, default: 1)
- `pageSize` (optional, default: 50)

**Example:**
```bash
curl "http://localhost:5000/api/tle/GPS"
```

#### 3. GET /api/tle/{system}/{prn}

Get TLE data for a specific satellite (most recent by default).

**Path Parameters:**
- `system` - Satellite system
- `prn` - PRN identifier

**Query Parameters:**
- `datetime` (optional) - ISO 8601 datetime

**Example:**
```bash
curl "http://localhost:5000/api/tle/GPS/G01"
curl "http://localhost:5000/api/tle/GPS/G01?datetime=2024-10-30T12:00:00Z"
```

**Response:**
```json
{
  "system": "GPS",
  "prn": "G01",
  "epoch": "2024-10-30T12:00:00.0000000Z",
  "line1": "1 37753U 11036A   24304.50000000  .00000012  00000-0  00000+0 0  9993",
  "line2": "2 37753  55.0000 195.0000 0000001  30.0000 330.0000  2.00000000 12345"
}
```

#### 4. GET /api/tle/{system}/{prn}/{datetime}

Get TLE data for a specific satellite at a specific time.

**Path Parameters:**
- `system` - Satellite system
- `prn` - PRN identifier
- `datetime` - ISO 8601 datetime

**Example:**
```bash
curl "http://localhost:5000/api/tle/GPS/G01/2024-10-30T12:00:00Z"
```

## Error Responses

### 400 Bad Request

Invalid input parameters:
```json
{
  "error": "Invalid system. Must be one of: GPS, GLONASS, GALILEO, BEIDOU"
}
```

### 404 Not Found

No data found:
```json
{
  "error": "No TLE data found for GPS/G01"
}
```

### 500 Internal Server Error

Server error:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500
}
```

## Environment Variables

All configuration can be overridden with environment variables:

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__Default` | PostgreSQL connection string | `Host=localhost;Database=meolut;Username=postgres;Password=postgres` |
| `Tle__TableName` | TLE table name | `tle` |
| `Swagger__Enabled` | Enable Swagger UI in production | `true` or `false` |
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development`, `Staging`, `Production` |
| `ASPNETCORE_URLS` | URLs to listen on | `http://0.0.0.0:5000` |

## Development

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Code Structure

```
src/TleApi/
├── Configuration/       # Configuration options
├── Data/               # Database context and repository
├── DTOs/               # Data transfer objects
├── Models/             # Entity models
├── Validators/         # Input validation
├── Program.cs          # Application entry point
├── appsettings.json    # Configuration
└── README.md           # This file
```

## Logging

Logs are written to:
- Console (stdout)
- File: `logs/tleapi-YYYYMMDD.log` (daily rolling)

Configure log levels in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

## Security Considerations

- All database queries are **parameterized** to prevent SQL injection
- Connection strings should be stored in **environment variables** or secure configuration stores in production
- The API is **read-only** and does not modify data
- Consider using **HTTPS** in production
- Implement **rate limiting** if exposing the API publicly

## License

See the LICENSE file in the repository root.
