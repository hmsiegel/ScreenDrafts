<#
.SYNOPSIS
  Downloads drafter avatars from the ScreenDrafts wiki using the pre-fetched
  image URLs in the CSV produced by Check-WikiPages.ps1.
  Saves each file as {publicId}.jpg and updates the DB.

.PARAMETER CsvPath
  Path to the wiki-check CSV. Default: looks for the most recent one in the
  same folder as this script.

.PARAMETER AvatarDir
  Destination folder.

.PARAMETER DryRun
  Logs planned actions without downloading or updating the DB.
#>
param(
  [string]$CsvPath    = "",
  [string]$AvatarDir  = "C:\Repos\ScreenDrafts\src\screendrafts.ui\public\drafters",
  [string]$PgHost     = "localhost",
  [string]$PgPort     = "5432",
  [string]$PgDatabase = "screendrafts",
  [string]$PgUser     = "postgres",
  [string]$PgPassword = "postgres",
  [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Resolve CSV path ──────────────────────────────────────────────────────────

if (-not $CsvPath) {
  $CsvPath = Get-ChildItem -Path $PSScriptRoot -Filter "wiki-check-*.csv" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1 -ExpandProperty FullName
}

if (-not $CsvPath -or -not (Test-Path $CsvPath)) {
  throw "No wiki-check CSV found. Run Check-WikiPages.ps1 first, or pass -CsvPath explicitly."
}

Write-Host "Using CSV: $CsvPath" -ForegroundColor Cyan

# ── Locate psql.exe ───────────────────────────────────────────────────────────

$psql = Get-Command psql -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty Source

if (-not $psql) {
  $candidates = @(
    "C:\Program Files\PostgreSQL\18\bin\psql.exe",
    "C:\Program Files\PostgreSQL\17\bin\psql.exe",
    "C:\Program Files\PostgreSQL\16\bin\psql.exe",
    "C:\Program Files\PostgreSQL\15\bin\psql.exe"
  )
  $psql = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

if (-not $psql) {
  throw "psql.exe not found. Add PostgreSQL bin to PATH or install PostgreSQL client tools."
}

$env:PGPASSWORD = $PgPassword
$psqlArgs = @("-h", $PgHost, "-p", $PgPort, "-U", $PgUser, "-d", $PgDatabase, "-t", "-A")

function Invoke-Psql([string]$sql) {
  $output = & $psql @psqlArgs -c $sql 2>&1
  if ($LASTEXITCODE -ne 0) { throw "psql error: $output" }
  return $output
}

# ── Load CSV and filter to rows with images ───────────────────────────────────

$rows = Import-Csv -Path $CsvPath |
  Where-Object { $_.HasPage -eq 'True' -and $_.ImageUrl -ne '' }

Write-Host "Found $($rows.Count) drafters with wiki images`n" -ForegroundColor Cyan

# ── Download ──────────────────────────────────────────────────────────────────

if (-not $DryRun) {
  [System.IO.Directory]::CreateDirectory($AvatarDir) | Out-Null
}

$results    = @()
$downloaded = 0
$skipped    = 0
$errors     = 0

foreach ($row in $rows) {
  # All wiki images are JPEG regardless of the URL extension
  $fileName = "$($row.PublicId).jpg"
  $destPath = Join-Path $AvatarDir $fileName

  # Skip if already downloaded
  if (Test-Path $destPath) {
    Write-Host "  SKIP (exists) $fileName" -ForegroundColor DarkGray
    $results += [PSCustomObject]@{
      Name     = $row.Name
      PublicId = $row.PublicId
      FileName = $fileName
      Status   = 'SKIPPED'
      Error    = ''
    }
    $skipped++
    continue
  }

  if ($DryRun) {
    Write-Host "  [DRY RUN] $($row.Name) → $fileName" -ForegroundColor Yellow
    $results += [PSCustomObject]@{
      Name     = $row.Name
      PublicId = $row.PublicId
      FileName = $fileName
      Status   = 'DRY_RUN'
      Error    = ''
    }
    $downloaded++
    continue
  }

  try {
    $response = Invoke-WebRequest -Uri $row.ImageUrl -UseBasicParsing -TimeoutSec 20
    [System.IO.File]::WriteAllBytes($destPath, $response.Content)

    $escaped  = $fileName.Replace("'", "''")
    $pubIdEsc = $row.PublicId.Replace("'", "''")
    Invoke-Psql "UPDATE drafts.people SET profile_picture_path = '$escaped' WHERE public_id = '$pubIdEsc';" | Out-Null

    Write-Host "  ✓ $($row.Name) → $fileName" -ForegroundColor Green
    $results += [PSCustomObject]@{
      Name     = $row.Name
      PublicId = $row.PublicId
      FileName = $fileName
      Status   = 'DOWNLOADED'
      Error    = ''
    }
    $downloaded++
    Start-Sleep -Milliseconds 200
  } catch {
    $err = $_.Exception.Message
    Write-Host "  ✗ $($row.Name) — $err" -ForegroundColor Red
    $results += [PSCustomObject]@{
      Name     = $row.Name
      PublicId = $row.PublicId
      FileName = $fileName
      Status   = 'ERROR'
      Error    = $err
    }
    $errors++
  }
}

# ── Missing summary ───────────────────────────────────────────────────────────

$missing = Import-Csv -Path $CsvPath | Where-Object { $_.HasPage -eq 'False' }
if ($missing.Count -gt 0) {
  Write-Host "`nNo wiki page found for:" -ForegroundColor Yellow
  $missing | ForEach-Object { Write-Host "  - $($_.Name) ($($_.PublicId))" -ForegroundColor Yellow }
}

# ── Log ───────────────────────────────────────────────────────────────────────

$logPath = Join-Path $PSScriptRoot "avatar-download-$(Get-Date -Format 'yyyyMMdd-HHmmss').csv"
$results | Export-Csv -Path $logPath -NoTypeInformation -Encoding UTF8

Write-Host ""
Write-Host "─────────────────────────────────────────" -ForegroundColor Cyan
if ($DryRun) { Write-Host "  DRY RUN — no files downloaded" -ForegroundColor Yellow }
Write-Host "  Downloaded  : $downloaded" -ForegroundColor Green
Write-Host "  Skipped     : $skipped"
Write-Host "  Errors      : $errors" -ForegroundColor $(if ($errors -gt 0) { 'Red' } else { 'Green' })
Write-Host "  No wiki page: $($missing.Count)" -ForegroundColor $(if ($missing.Count -gt 0) { 'Yellow' } else { 'Green' })
Write-Host "  Log         : $logPath"
Write-Host "─────────────────────────────────────────" -ForegroundColor Cyan