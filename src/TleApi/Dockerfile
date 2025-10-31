# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/TleApi/TleApi.csproj", "TleApi/"]
RUN dotnet restore "TleApi/TleApi.csproj"

# Copy source code and build
COPY src/TleApi/ TleApi/
WORKDIR /src/TleApi
RUN dotnet build "TleApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "TleApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TleApi.dll"]
