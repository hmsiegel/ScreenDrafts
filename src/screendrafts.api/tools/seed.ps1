param(
   [Parameter(Mandatory=$true, Position=0)]
   [ValidateSet("movies", "drafts")]
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

# -- Wait fos docker to be ready ---
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

# --- Run Seeder ---
Write-Host "`n🚀 Running $seederService $moduleArg..." -ForegroundColor Cyan
docker compose -f $composeBase -f $composeSeeding run --rm $seederService $moduleArg

Write-Host "`n✅ Seeding complete." -ForegroundColor Green

# --- Clean up containers ---
Write-Host "`n🧹 Stopping and removing all seeding containers..." -ForegroundColor Cyan
docker compose -f $composeBase -f $composeSeeding down --volumes --remove-orphans

Write-Host "✅ Cleanup complete." -ForegroundColor Green