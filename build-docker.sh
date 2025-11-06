#!/bin/bash
set -e

echo "Building EphemerisHub Docker Image..."
echo ""

# Try the standard multi-stage build first
echo "Attempting standard multi-stage Docker build..."
if docker build -t ephemerishub:latest -f Dockerfile . 2>&1 | tee /tmp/docker-build.log; then
    echo ""
    echo "✓ Docker image built successfully using standard Dockerfile"
    echo ""
    exit 0
fi

# If standard build failed, check if it was due to SSL/NuGet issues
if grep -q "NU1301\|SSL connection could not be established" /tmp/docker-build.log; then
    echo ""
    echo "⚠ Standard build failed due to SSL/NuGet certificate issues"
    echo "  Falling back to pre-build method..."
    echo ""
    
    # Build the application on the host first
    echo "Building application on host..."
    dotnet publish -c Release -o src/EphemerisHub/bin/Release/net9.0/publish src/EphemerisHub/EphemerisHub.csproj
    
    # Build Docker image with pre-built binaries
    echo ""
    echo "Building Docker image with pre-built binaries..."
    docker build -t ephemerishub:latest -f Dockerfile.prebuild .
    
    echo ""
    echo "✓ Docker image built successfully using pre-build method"
    echo ""
else
    echo ""
    echo "✗ Docker build failed with unexpected error"
    echo "  Check /tmp/docker-build.log for details"
    exit 1
fi
