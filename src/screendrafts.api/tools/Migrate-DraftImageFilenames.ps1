<#
.SYNOPSIS
  Renames draft episode images from title-based filenames to publicId-based
  filenames and updates drafts.drafts.image_path in PostgreSQL.

.PARAMETER DryRun
  If set, logs all planned actions without renaming files or updating the DB.

.PARAMETER EpisodeDir
  Path to the folder containing episode image files.
  Default: screendrafts.ui\public\episodes  (relative to repo root)

.PARAMETER DestDir
  Destination folder for renamed files (served by the API).
  Default: ScreenDrafts.Web\wwwroot\drafts

.PARAMETER PgHost / PgPort / PgDatabase / PgUser / PgPassword
  PostgreSQL connection parameters.

.EXAMPLE
  # Preview — no changes
  .\Migrate-DraftImageFilenames.ps1 -DryRun

  # Run for real
  .\Migrate-DraftImageFilenames.ps1
#>
param(
  [switch]$DryRun,
  [string]$EpisodeDir  = "C:\Repos\ScreenDrafts\src\screendrafts.ui\public\episodes",
  [string]$DestDir     = "C:\Repos\ScreenDrafts\src\screendrafts.api\src\api\ScreenDrafts.Web\wwwroot\drafts",
  [string]$PgHost      = "localhost",
  [string]$PgPort      = "5432",
  [string]$PgDatabase  = "screendrafts",
  [string]$PgUser      = "postgres",
  [string]$PgPassword  = "postgres"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

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

Write-Host "Using psql: $psql" -ForegroundColor Cyan

$env:PGPASSWORD = $PgPassword
$psqlArgs = @("-h", $PgHost, "-p", $PgPort, "-U", $PgUser, "-d", $PgDatabase, "-t", "-A")

function Invoke-Psql([string]$sql) {
  $output = & $psql @psqlArgs -c $sql 2>&1
  if ($LASTEXITCODE -ne 0) { throw "psql error: $output" }
  return $output
}

# ── Load all Draft records ────────────────────────────────────────────────────

Write-Host "Loading drafts from database..." -ForegroundColor Cyan

$raw = Invoke-Psql @"
SELECT public_id, title, image_path
FROM drafts.drafts;
"@

# Build lookup: normalised title → public_id
$titleLookup = @{}   # lower title → public_id
$byFile      = @{}   # current image_path stem → public_id

foreach ($line in ($raw -split "`n" | Where-Object { $_.Trim() -ne '' })) {
  $parts = $line -split '\|'
  if ($parts.Count -lt 2) { continue }

  $publicId  = $parts[0].Trim()
  $title     = $parts[1].Trim()
  $imagePath = if ($parts.Count -ge 3) { $parts[2].Trim() } else { '' }

  $key = $title.ToLower()
  if (-not $titleLookup.ContainsKey($key)) {
    $titleLookup[$key] = $publicId
  }

  if ($imagePath -ne '') {
    $stem = [System.IO.Path]::GetFileNameWithoutExtension($imagePath).ToLower()
    if (-not $byFile.ContainsKey($stem)) {
      $byFile[$stem] = $publicId
    }
  }
}

Write-Host "  Loaded $($titleLookup.Count) drafts" -ForegroundColor Cyan

# ── Ensure destination directory exists ──────────────────────────────────────

if (-not $DryRun) {
  New-Item -ItemType Directory -Force -Path $DestDir | Out-Null
}

# ── Process files ─────────────────────────────────────────────────────────────

$files = Get-ChildItem -Path $EpisodeDir -File |
  Where-Object { $_.Extension -iin @('.jpg', '.jpeg', '.png', '.webp') }

Write-Host "Found $($files.Count) episode image files`n" -ForegroundColor Cyan

$results = @()
$matched = 0
$skipped = 0
$noMatch = 0

foreach ($file in $files) {
  $stem = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
  $ext  = $file.Extension.ToLower().TrimStart('.')
  # Normalise .jpeg → .jpg
  if ($ext -eq 'jpeg') { $ext = 'jpg' }

  $publicId = $null

  # Strategy 1: stem is already a publicId (already migrated)
  $allPublicIds = $titleLookup.Values
  if ($allPublicIds -contains $stem) {
    $publicId = $stem
  }

  # Strategy 2: stem matches existing image_path in DB
  if (-not $publicId -and $byFile.ContainsKey($stem.ToLower())) {
    $publicId = $byFile[$stem.ToLower()]
  }

  # Strategy 3: stem IS the draft title (original case, no transformation needed)
  if (-not $publicId) {
    $key = $stem.ToLower()
    if ($titleLookup.ContainsKey($key)) {
      $publicId = $titleLookup[$key]
    }
  }

  if (-not $publicId) {
    Write-Warning "  NO MATCH: $($file.Name)"
    $results += [PSCustomObject]@{
      OriginalFile = $file.Name
      Title        = $stem
      PublicId     = ''
      NewFile      = ''
      Status       = 'NO_MATCH'
      Error        = ''
    }
    $noMatch++
    continue
  }

  $newFileName = "$publicId.$ext"
  $newFilePath = Join-Path $DestDir $newFileName

  # Already correct and in destination
  if ((Join-Path $DestDir $newFileName | Test-Path) -and $file.Name -eq $newFileName) {
    $results += [PSCustomObject]@{
      OriginalFile = $file.Name
      Title        = $stem
      PublicId     = $publicId
      NewFile      = $newFileName
      Status       = 'ALREADY_DONE'
      Error        = ''
    }
    $skipped++
    continue
  }

  if ($DryRun) {
    Write-Host "  [DRY RUN] $($file.Name) → $newFileName" -ForegroundColor Yellow
    $results += [PSCustomObject]@{
      OriginalFile = $file.Name
      Title        = $stem
      PublicId     = $publicId
      NewFile      = $newFileName
      Status       = 'DRY_RUN'
      Error        = ''
    }
    $matched++
    continue
  }

  $err = ''
  try {
    # Remove existing file at destination if different extension
    foreach ($existingExt in @('jpg', 'png', 'webp')) {
      $existing = Join-Path $DestDir "$publicId.$existingExt"
      if ((Test-Path $existing) -and $existingExt -ne $ext) {
        Remove-Item $existing -Force
        Write-Warning "  Removed old $publicId.$existingExt"
      }
    }

    # Copy (not move) to destination with new name
    Copy-Item -Path $file.FullName -Destination $newFilePath -Force

    $escaped  = $newFileName.Replace("'", "''")
    $pubIdEsc = $publicId.Replace("'", "''")
    Invoke-Psql "UPDATE drafts.drafts SET image_path = '$escaped' WHERE public_id = '$pubIdEsc';" | Out-Null

    Write-Host "  OK  $($file.Name) -> $newFileName" -ForegroundColor Green
    $matched++

    $results += [PSCustomObject]@{
      OriginalFile = $file.Name
      Title        = $stem
      PublicId     = $publicId
      NewFile      = $newFileName
      Status       = 'MIGRATED'
      Error        = ''
    }
  } catch {
    $err = $_.Exception.Message
    Write-Warning "  ERROR: $($file.Name) — $err"
    $results += [PSCustomObject]@{
      OriginalFile = $file.Name
      Title        = $stem
      PublicId     = $publicId
      NewFile      = $newFileName
      Status       = 'ERROR'
      Error        = $err
    }
  }
}

# ── Summary ───────────────────────────────────────────────────────────────────

$logPath = Join-Path $PSScriptRoot "draft-image-migration-$(Get-Date -Format 'yyyyMMdd-HHmmss').csv"
$results | Export-Csv -Path $logPath -NoTypeInformation -Encoding UTF8

Write-Host ""
Write-Host "-----------------------------------------" -ForegroundColor Cyan
if ($DryRun) { Write-Host "  DRY RUN - no files copied, no DB changes" -ForegroundColor Yellow }
Write-Host "  Migrated / Would migrate : $matched" -ForegroundColor Green
Write-Host "  Already done (skipped)   : $skipped"
Write-Host "  No DB match              : $noMatch" -ForegroundColor $(if ($noMatch -gt 0) { 'Red' } else { 'Green' })
Write-Host "  Log written to           : $logPath"
Write-Host "-----------------------------------------" -ForegroundColor Cyan