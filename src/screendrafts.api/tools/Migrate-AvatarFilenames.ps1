<#
.SYNOPSIS
  Renames avatar files from display-name slugs to publicId-based filenames
  and updates drafts.people.profile_picture_path in PostgreSQL.

.PARAMETER DryRun
  If set, logs all planned actions without renaming files or updating the DB.

.PARAMETER AvatarDir
  Path to the folder containing avatar files.

.PARAMETER PgHost / PgPort / PgDatabase / PgUser / PgPassword
  PostgreSQL connection parameters.

.EXAMPLE
  # Preview — no changes
  .\Migrate-AvatarFilenames.ps1 -DryRun

  # Run for real
  .\Migrate-AvatarFilenames.ps1
#>
param(
  [switch]$DryRun,
  [string]$AvatarDir    = "C:\Repos\ScreenDrafts\src\screendrafts.ui\public\drafters",
  [string]$PgHost       = "localhost",
  [string]$PgPort       = "5432",
  [string]$PgDatabase   = "screendrafts",
  [string]$PgUser       = "postgres",
  [string]$PgPassword   = "postgres"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Locate psql.exe ───────────────────────────────────────────────────────────

$psql = Get-Command psql -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty Source

if (-not $psql) {
  # Common install locations
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
  if ($LASTEXITCODE -ne 0) {
    throw "psql error: $output"
  }
  return $output
}

# ── Load all Person records ───────────────────────────────────────────────────

Write-Host "Loading people from database..." -ForegroundColor Cyan

$raw = Invoke-Psql @"
SELECT public_id, COALESCE(display_name, first_name || ' ' || last_name), profile_picture_path
FROM drafts.people;
"@

# psql -A -t outputs: col1|col2|col3 per line
$lookup = @{}   # normalised display name → public_id
$byFile = @{}   # current filename (no ext) → public_id

foreach ($line in ($raw -split "`n" | Where-Object { $_.Trim() -ne '' })) {
  $parts = $line -split '\|'
  if ($parts.Count -lt 2) { continue }

  $publicId    = $parts[0].Trim()
  $displayName = $parts[1].Trim()
  $dbFile      = if ($parts.Count -ge 3) { $parts[2].Trim() } else { '' }

  $key = $displayName.ToLower()
  if (-not $lookup.ContainsKey($key)) {
    $lookup[$key] = $publicId
  }

  # Also index by current filename stem so we can match files already in DB
  if ($dbFile -ne '') {
    $stem = [System.IO.Path]::GetFileNameWithoutExtension($dbFile).ToLower()
    if (-not $byFile.ContainsKey($stem)) {
      $byFile[$stem] = $publicId
    }
  }
}

Write-Host "  Loaded $($lookup.Count) people" -ForegroundColor Cyan

# ── Helper: slug → display name ───────────────────────────────────────────────

function ToDisplayName([string]$stem) {
  if ($stem -match '-') {
    return ($stem -split '-' | ForEach-Object {
      $_.Substring(0,1).ToUpper() + $_.Substring(1).ToLower()
    }) -join ' '
  }
  return $stem  # already title-cased
}

# ── Process files ─────────────────────────────────────────────────────────────

$files = Get-ChildItem -Path $AvatarDir -File |
  Where-Object { $_.Extension -iin @('.webp', '.jpg', '.jpeg', '.png') }

Write-Host "Found $($files.Count) avatar files`n" -ForegroundColor Cyan

$results  = @()
$matched  = 0
$skipped  = 0
$noMatch  = 0

foreach ($file in $files) {
  $stem = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
  $ext  = $file.Extension.ToLower().TrimStart('.')

  # Resolve publicId — try three strategies in order:
  # 1. File stem IS already a publicId (already migrated)
  # 2. File stem matches a current DB filename stem
  # 3. File stem → display name → lookup
  $publicId = $null

  # Strategy 1: stem looks like a NanoId public ID (contains underscore prefix e.g. "p_abc")
  $allPublicIds = $lookup.Values
  if ($allPublicIds -contains $stem) {
    $publicId = $stem
  }

  # Strategy 2: current DB file stem match
  if (-not $publicId -and $byFile.ContainsKey($stem.ToLower())) {
    $publicId = $byFile[$stem.ToLower()]
  }

  # Strategy 3: derive display name from slug and look up
  if (-not $publicId) {
    $displayName = ToDisplayName $stem
    $key = $displayName.ToLower()
    if ($lookup.ContainsKey($key)) {
      $publicId = $lookup[$key]
    }
  }

  if (-not $publicId) {
    Write-Warning "  NO MATCH: $($file.Name)"
    $results += [PSCustomObject]@{
      OriginalFile = $file.Name
      DisplayName  = (ToDisplayName $stem)
      PublicId     = ''
      NewFile      = ''
      Status       = 'NO_MATCH'
      Error        = ''
    }
    $noMatch++
    continue
  }

  $newFileName = "$publicId.$ext"
  $newFilePath = Join-Path $AvatarDir $newFileName

  # Already correct
  if ($file.Name -eq $newFileName) {
    $results += [PSCustomObject]@{
      OriginalFile = $file.Name
      DisplayName  = (ToDisplayName $stem)
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
      DisplayName  = (ToDisplayName $stem)
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
    if (Test-Path $newFilePath) {
      Remove-Item $newFilePath -Force
      Write-Warning "  Overwrote existing $newFileName"
    }
    Rename-Item -Path $file.FullName -NewName $newFileName -Force

    $escaped  = $newFileName.Replace("'", "''")
    $pubIdEsc = $publicId.Replace("'", "''")
    Invoke-Psql "UPDATE drafts.people SET profile_picture_path = '$escaped' WHERE public_id = '$pubIdEsc';" | Out-Null

    Write-Host "  ✓ $($file.Name) → $newFileName" -ForegroundColor Green
    $matched++

    $results += [PSCustomObject]@{
      OriginalFile = $file.Name
      DisplayName  = (ToDisplayName $stem)
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
      DisplayName  = (ToDisplayName $stem)
      PublicId     = $publicId
      NewFile      = $newFileName
      Status       = 'ERROR'
      Error        = $err
    }
  }
}

# ── Summary ───────────────────────────────────────────────────────────────────

$logPath = Join-Path $PSScriptRoot "avatar-migration-$(Get-Date -Format 'yyyyMMdd-HHmmss').csv"
$results | Export-Csv -Path $logPath -NoTypeInformation -Encoding UTF8

Write-Host ""
Write-Host "─────────────────────────────────────────" -ForegroundColor Cyan
if ($DryRun) { Write-Host "  DRY RUN — no files renamed, no DB changes" -ForegroundColor Yellow }
Write-Host "  Migrated / Would migrate : $matched" -ForegroundColor Green
Write-Host "  Already done (skipped)   : $skipped"
Write-Host "  No DB match              : $noMatch" -ForegroundColor $(if ($noMatch -gt 0) { 'Red' } else { 'Green' })
Write-Host "  Log written to           : $logPath"
Write-Host "─────────────────────────────────────────" -ForegroundColor Cyan