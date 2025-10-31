# TLE API Implementation Summary

## Overview
This implementation adds a complete .NET 9 ASP.NET Core Minimal API project for reading Two-Line Element (TLE) data from a PostgreSQL database, fulfilling the requirements from issue #11.

## What Was Implemented

### 1. Project Structure
- Created `src/TleApi` - Main API project (.NET 9 Minimal API)
- Created `src/TleApi.Tests` - Unit test project with xUnit
- Both projects added to `meolut.sln` and properly nested under the `src` folder

### 2. Core Components

#### Models and DTOs
- **TleEntity** (`Models/TleEntity.cs`) - Database entity mapping to PostgreSQL table
- **TleDto** (`DTOs/TleDto.cs`) - Data transfer object for API responses
- **PagedResult<T>** (`DTOs/PagedResult.cs`) - Generic wrapper for paginated responses

#### Data Access Layer
- **TleDbContext** (`Data/TleDbContext.cs`) - EF Core DbContext with configurable table name
- **ITleRepository** / **TleRepository** (`Data/`) - Repository pattern for data access
- All queries are parameterized to prevent SQL injection

#### Validation
- **TleValidator** (`Validators/TleValidator.cs`) - Static validation methods
- **TleValidationConstants** (`Validators/TleValidationConstants.cs`) - Validation constants
- Validates: system (GPS, GLONASS, GALILEO, BEIDOU), PRN format, ISO 8601 datetime, pagination

#### Configuration
- **TleOptions** (`Configuration/TleOptions.cs`) - Configuration options for table name
- Uses IOptions pattern for proper DI integration

### 3. API Endpoints

All endpoints are under `/api/tle` and support:
- Query parameter filtering
- ISO 8601 datetime filtering
- Pagination (1-200 items per page)
- Proper HTTP status codes (200, 400, 404, 500)

1. **GET /api/tle**
   - List all TLE data with optional filtering
   - Query params: system, prn, datetime, page, pageSize

2. **GET /api/tle/{system}**
   - List TLE data for a specific satellite system
   - Query params: datetime, page, pageSize

3. **GET /api/tle/{system}/{prn}**
   - Get TLE for a specific satellite (latest or at specific datetime)
   - Query params: datetime

4. **GET /api/tle/{system}/{prn}/{datetime}**
   - Get TLE for a specific satellite at a specific time (path parameter)

### 4. OpenAPI/Swagger Documentation

- Full Swagger/OpenAPI v3 documentation using Swashbuckle.AspNetCore
- Swagger UI available at root URL (`/`) in development
- Configurable via `Swagger:Enabled` in appsettings.json
- Includes:
  - Endpoint descriptions and summaries
  - Parameter documentation
  - Response codes and schemas
  - Request/response examples

### 5. Configuration

#### Database Connection
- ConnectionString in `appsettings.json` under `ConnectionStrings:Default`
- Overridable via environment variable: `ConnectionStrings__Default`

#### Table Configuration
- Table name configurable via `Tle:TableName` (default: "tle")
- Overridable via environment variable: `Tle__TableName`

#### Expected Database Schema
```sql
Table: tle (or configured name)
Columns:
  - system (text/varchar) - GPS, GLONASS, GALILEO, BEIDOU
  - prn (text/varchar) - PRN identifier
  - epoch (timestamp with time zone) - TLE epoch/validity time
  - line1 (text) - TLE line 1
  - line2 (text) - TLE line 2
```

### 6. Logging

- Structured logging with Serilog
- Logs to console and file (`logs/tleapi-YYYYMMDD.log`)
- Request logging middleware for HTTP requests
- Configurable log levels in appsettings.json

### 7. Testing

- 32 unit tests covering all validation logic
- Tests for:
  - System validation (valid/invalid satellite systems)
  - PRN validation (length, whitespace)
  - DateTime parsing and validation
  - Pagination validation
- All tests passing

### 8. Docker Support

- **Dockerfile** - Multi-stage Docker build
  - Build stage with .NET 9 SDK
  - Runtime stage with .NET 9 ASP.NET runtime
  - Exposes port 8080

- **docker-compose.yml** - Complete local development stack
  - PostgreSQL 16 service
  - TLE API service
  - Health checks and dependency management

### 9. Documentation

- **README.md** - Comprehensive documentation including:
  - Features overview
  - Prerequisites
  - Configuration guide
  - Database schema
  - API endpoint documentation with examples
  - Error response examples
  - Environment variables
  - Docker instructions
  - Security considerations

- **TleApi.http** - HTTP request examples for testing with REST clients

- **.env.example** - Example environment variables file

### 10. NuGet Packages

Production dependencies:
- `Npgsql.EntityFrameworkCore.PostgreSQL` 9.0.2 - PostgreSQL EF Core provider
- `Swashbuckle.AspNetCore` 7.2.0 - OpenAPI/Swagger
- `FluentValidation.AspNetCore` 11.3.0 - Validation framework
- `Serilog.AspNetCore` 9.0.0 - Structured logging

Test dependencies:
- `xUnit` - Test framework
- `Moq` 4.20.72 - Mocking library

## Security Features

1. **Parameterized Queries** - All database queries use EF Core with parameters, preventing SQL injection
2. **Input Validation** - Comprehensive validation on all inputs
3. **Read-Only API** - No data modification endpoints
4. **Configuration Security** - Sensitive data (connection strings) in environment variables
5. **CodeQL Analysis** - Passed with 0 security alerts

## Acceptance Criteria ✓

All criteria from issue #11 met:

- ✓ Реализованы эндпоинты /api/tle, /api/tle/{system}, /api/tle/{system}/{prn}, /api/tle/{system}/{prn}/{datetime}
- ✓ Данные читаются из PostgreSQL, без обращений к внешним источникам
- ✓ OpenAPI/Swagger описывает все эндпоинты, параметры, схемы и коды ошибок
- ✓ Валидация входных параметров и корректные HTTP-коды (200/400/404/500)
- ✓ Документация по запуску и конфигурации присутствует

## How to Use

### Local Development
```bash
cd src/TleApi
dotnet run
```
Navigate to http://localhost:5015 to access Swagger UI

### With Docker Compose
```bash
cd src/TleApi
docker-compose up
```
API available at http://localhost:5000

### Running Tests
```bash
dotnet test
```

## Project Quality

- ✓ Builds successfully on .NET 9
- ✓ All 32 unit tests passing
- ✓ No CodeQL security vulnerabilities
- ✓ Code reviewed and issues addressed
- ✓ Follows .NET coding conventions
- ✓ Comprehensive documentation
- ✓ Docker support for easy deployment

## Files Changed

### New Files (19)
1. `src/TleApi/TleApi.csproj` - Project file
2. `src/TleApi/Program.cs` - Application entry point with endpoint mapping
3. `src/TleApi/Models/TleEntity.cs` - Database entity
4. `src/TleApi/DTOs/TleDto.cs` - Response DTO
5. `src/TleApi/DTOs/PagedResult.cs` - Pagination wrapper
6. `src/TleApi/Data/TleDbContext.cs` - EF Core context
7. `src/TleApi/Data/ITleRepository.cs` - Repository interface
8. `src/TleApi/Data/TleRepository.cs` - Repository implementation
9. `src/TleApi/Validators/TleValidator.cs` - Validation logic
10. `src/TleApi/Validators/TleValidationConstants.cs` - Validation constants
11. `src/TleApi/Configuration/TleOptions.cs` - Configuration options
12. `src/TleApi/appsettings.json` - Application configuration
13. `src/TleApi/appsettings.Development.json` - Development configuration
14. `src/TleApi/README.md` - Project documentation
15. `src/TleApi/Dockerfile` - Docker image definition
16. `src/TleApi/docker-compose.yml` - Docker Compose stack
17. `src/TleApi/.env.example` - Environment variable example
18. `src/TleApi/TleApi.http` - HTTP request examples
19. `src/TleApi/Properties/launchSettings.json` - Launch configuration

### New Test Files (2)
1. `src/TleApi.Tests/TleApi.Tests.csproj` - Test project file
2. `src/TleApi.Tests/TleValidatorTests.cs` - Validation unit tests

### Modified Files (1)
1. `meolut.sln` - Updated to include new projects

## Technical Highlights

1. **Clean Architecture** - Separation of concerns with Models, DTOs, Data, Validators, Configuration
2. **Dependency Injection** - Proper use of ASP.NET Core DI with IOptions pattern
3. **SOLID Principles** - Interface-based design (ITleRepository), single responsibility
4. **Testability** - Repository pattern enables easy testing with mocks
5. **Configuration Management** - Flexible configuration with appsettings and environment variables
6. **Error Handling** - Consistent error responses with proper status codes
7. **Documentation** - OpenAPI/Swagger for API documentation, comprehensive README

## Next Steps (Optional)

The following could be added in future iterations:
1. Integration tests with a test database
2. Performance testing and optimization
3. API versioning support
4. Rate limiting middleware
5. CORS configuration for web clients
6. Health check endpoints
7. Metrics and monitoring (Prometheus/Grafana)
8. CI/CD pipeline configuration
