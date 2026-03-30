param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("movies", "drafts", "users", "honorifics")]
    [string]$Seeder,

    [string]$Module,

    [switch]$Rebuild
)

$ErrorActionPreference = "Stop"

# --- Resolve paths base on the current script location ---
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$rootDir = Resolve-Path "$scriptDir/.."

$composeBase = "$rootDir/docker-compose.yml"
$composeSeeding = "$rootDir/docker-compose.seeding.yml"
$seederService = "screendrafts.seeding.$Seeder"

# -- Wait for docker to be ready ---
function Wait-ForPostgres {
    Write-Host "`n⏳ Waiting for PostgreSQL to be ready..." -ForegroundColor Cyan
    do {
        $ready = docker exec screendraftsapi-screendrafts.database-1 pg_isready -U postgres 2>$null
        if ($LASTEXITCODE -ne 0) {
            Start-Sleep -Seconds 1
        }
    } while ($LASTEXITCODE -ne 0)
    Write-Host "✅ PostgreSQL is ready." -ForegroundColor Green
}

# -- Wait for Keycloak to be ready ---
function Wait-ForKeycloak {
    $timeout = 60
    $elapsed = 0

    Write-Host "`n⏳ Waiting for Keycloak to be ready..." -ForegroundColor Cyan

    # Wait for port to be available
    do {
        $portCheck = Test-NetConnection -ComputerName localhost -Port 18080 -WarningAction SilentlyContinue
        if ($portCheck.TcpTestSucceeded) {
            break
        }
        Write-Host "⏳ Waiting for Keycloak port..." -ForegroundColor Yellow
        Start-Sleep -Seconds 2
        $elapsed += 2

        if ($elapsed -ge $timeout) {
            Write-Host "❌ Keycloak port did not become available within the timeout period." -ForegroundColor Red
            exit 1
        }
    } while ($true)

    # Give Keycloak additional time to fully initialize
    Write-Host "⏳ Keycloak port is ready, waiting for full initialization..." -ForegroundColor Yellow

    $waitTime = 60

    for ($i = 0; $i -lt $waitTime; $i++) {
        Start-Sleep -Seconds 1
        Write-Progress -Activity "Waiting for Keycloak initialization" -Status "Elapsed: $($i + 1) seconds" -CurrentOperation "Checking Keycloak readiness..." -PercentComplete (($i + 1) / $waitTime * 100)
    }

    Write-Host "✅ Keycloak is ready." -ForegroundColor Green
}

# --- Build optional module argument ---
$moduleArg = ""
if ($Module) {
    $moduleArg = "--module=$Module"
}

# --- Optional Rebuild ---
if ($Rebuild) {
    Write-Host "`n🔧 Rebuilding seeder container..." -ForegroundColor Cyan
    docker compose -f $composeBase -f $composeSeeding build $seederService
}

# --- Start DB container only ---
Write-Host "`n🔄 Starting database container..." -ForegroundColor Cyan
docker compose -f $composeBase up -d screendrafts.database

Wait-ForPostgres

if ($Seeder -eq "users") {
    Write-Host "`n🔄 Starting keycloak container..." -ForegroundColor Cyan
    docker compose -f $composeBase up -d screendrafts.identity

    Wait-ForKeycloak
}

# --- Run Seeder ---
Write-Host "`n🚀 Running $seederService $moduleArg..." -ForegroundColor Cyan
docker compose -f $composeBase -f $composeSeeding run --rm $seederService $moduleArg

Write-Host "`n✅ Seeding complete." -ForegroundColor Green

# --- Wait a moment for any pending operations to complete ---
Write-Host "`n⏳ Waiting for any pending operations..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

# --- Clean up containers ---
Write-Host "`n🧹 Stopping and removing all seeding containers..." -ForegroundColor Cyan
docker compose -f $composeBase -f $composeSeeding down --volumes --remove-orphans

Write-Host "✅ Cleanup complete." -ForegroundColor Green