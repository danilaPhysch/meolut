# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files for dependency caching
COPY meolut.sln ./
COPY src/EphemerisHub/*.csproj ./src/EphemerisHub/

# Restore dependencies (cache this layer if project files haven't changed)
RUN dotnet restore src/EphemerisHub/EphemerisHub.csproj

# Copy the rest of the source code
COPY src/EphemerisHub/ ./src/EphemerisHub/

# Build and publish the application
WORKDIR /src/src/EphemerisHub
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create logs directory
RUN mkdir -p /app/logs

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "EphemerisHub.dll"]
