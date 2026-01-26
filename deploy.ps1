# Recommendo - One-Command Deployment Script
# This script builds and deploys the entire Recommendo stack

param(
    [switch]$Clean,
    [switch]$Build,
    [switch]$NoBuild,
    [switch]$Stop,
    [switch]$Down,
    [switch]$Logs
)

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Recommendo Deployment Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Check if Docker is running
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    docker info | Out-Null
    Write-Host "OK Docker is running" -ForegroundColor Green
} catch {
    Write-Host "ERROR Docker is not running. Please start Docker Desktop." -ForegroundColor Red
    exit 1
}

# Stop containers
if ($Stop -or $Down) {
    Write-Host ""
    Write-Host "Stopping containers..." -ForegroundColor Yellow
    docker-compose down
    Write-Host "OK Containers stopped" -ForegroundColor Green
    
    if ($Down) {
        Write-Host ""
        Write-Host "Removing volumes..." -ForegroundColor Yellow
        docker-compose down -v
        Write-Host "OK Volumes removed" -ForegroundColor Green
    }
    
    if (-not $Build -and -not $Clean) {
        exit 0
    }
}

# Clean build artifacts
if ($Clean) {
    Write-Host ""
    Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
    
    # Clean frontend
    if (Test-Path "frontend/node_modules") {
        Remove-Item -Recurse -Force "frontend/node_modules"
        Write-Host "  OK Removed frontend/node_modules" -ForegroundColor Green
    }
    if (Test-Path "frontend/dist") {
        Remove-Item -Recurse -Force "frontend/dist"
        Write-Host "  OK Removed frontend/dist" -ForegroundColor Green
    }
    
    # Clean API
    Get-ChildItem -Path "api" -Recurse -Include "bin","obj" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  OK Removed API bin/obj folders" -ForegroundColor Green
    
    Write-Host "OK Clean complete" -ForegroundColor Green
}

# Show logs
if ($Logs) {
    Write-Host ""
    Write-Host "Showing logs (Ctrl+C to exit)..." -ForegroundColor Yellow
    docker-compose logs -f
    exit 0
}

# Build and start
Write-Host ""
Write-Host "Building and starting containers..." -ForegroundColor Yellow
Write-Host ""

if ($NoBuild) {
    Write-Host "Skipping build (using existing images)..." -ForegroundColor Yellow
    docker-compose up -d
} else {
    Write-Host "Building images (this may take a few minutes)..." -ForegroundColor Yellow
    docker-compose up -d --build
}

# Wait for services to be healthy
Write-Host ""
Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check service status
Write-Host ""
Write-Host "Checking service status..." -ForegroundColor Yellow
try {
    $containersJson = docker-compose ps --format json 2>$null
    if ($containersJson) {
        $containers = $containersJson | ConvertFrom-Json
        
        $allHealthy = $true
        foreach ($container in $containers) {
            $status = $container.State
            $name = $container.Service
            
            if ($status -eq "running") {
                Write-Host "  OK $name is running" -ForegroundColor Green
            } else {
                Write-Host "  ERROR $name is $status" -ForegroundColor Red
                $allHealthy = $false
            }
        }
    } else {
        Write-Host "  INFO Could not get container status, checking with docker ps..." -ForegroundColor Yellow
        docker-compose ps
        $allHealthy = $true
    }
} catch {
    Write-Host "  INFO Status check skipped" -ForegroundColor Yellow
    $allHealthy = $true
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Deployment Complete!" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Services are available at:" -ForegroundColor Green
Write-Host "  Frontend:  http://localhost:5001" -ForegroundColor White
Write-Host "  API:       http://localhost:5002" -ForegroundColor White
Write-Host "  Database:  localhost:5003" -ForegroundColor White
Write-Host ""
Write-Host "Swagger API Documentation:" -ForegroundColor Green
Write-Host "  http://localhost:5002/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Useful commands:" -ForegroundColor Yellow
Write-Host "  .\deploy.ps1 -Logs       Show container logs" -ForegroundColor White
Write-Host "  .\deploy.ps1 -Stop       Stop all containers" -ForegroundColor White
Write-Host "  .\deploy.ps1 -Down       Stop and remove containers" -ForegroundColor White
Write-Host "  .\deploy.ps1 -Clean      Clean build artifacts" -ForegroundColor White
Write-Host "  .\deploy.ps1 -NoBuild    Start without rebuilding" -ForegroundColor White
Write-Host ""

if (-not $allHealthy) {
    Write-Host "WARNING Some services are not running correctly. Check logs with: .\deploy.ps1 -Logs" -ForegroundColor Yellow
    exit 1
}
