<#
.SYNOPSIS
  Finds every Patreon-related permission constant defined in the codebase's
  *Auth.cs classes, then emits a SQL script to check which of them (and which
  roles) actually exist in the database.

.DESCRIPTION
  Two passes:
    1. Scans *.cs files under -RepoRoot for permission constant declarations
       (the `internal const string Xxx = "some:code";` pattern used in the
       *Auth.cs classes) where either the constant name or its string value
       contains "patreon" (case-insensitive).
    2. Scans *.sql files under -RepoRoot for any historical INSERT referencing
       a Patreon-related permission code, or the literal role name 'Patreon',
       so you can see what was ever granted historically — including grants
       that may have been dropped when the users.* access-control tables were
       removed (migration 20260420175252_Drop_Users_Access_Controls).

  Writes:
    - Console table of every discovered permission constant (name, code, file, line)
    - patreon-permissions-found.csv         (pass 1 results)
    - patreon-permission-history.csv        (pass 2 results — sql references)
    - patreon-permissions-gap-check.sql     (ready to run in DBeaver)

.PARAMETER RepoRoot
  Root of the repo to scan. Defaults to the current directory.

.EXAMPLE
  ./find-patreon-permissions.ps1 -RepoRoot C:\src\ScreenDrafts
#>

[CmdletBinding()]
param(
  [string]$RepoRoot = ".",
  [string]$OutputDir = "."
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $RepoRoot)) {
  throw "RepoRoot '$RepoRoot' does not exist."
}

$RepoRoot = (Resolve-Path $RepoRoot).Path
Write-Host "Scanning $RepoRoot ..." -ForegroundColor Cyan

# ── Pass 1: permission constants in *.cs files ──────────────────────────────

$constPattern = 'internal\s+const\s+string\s+(?<name>\w+)\s*=\s*"(?<value>[^"]+)"'

$csResults = @()

Get-ChildItem -Path $RepoRoot -Recurse -Include *.cs -File |
  Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' } |
  ForEach-Object {
    $file = $_
    $lineNum = 0
    Get-Content -LiteralPath $file.FullName | ForEach-Object {
      $lineNum++
      $line = $_
      if ($line -match $constPattern) {
        $name = $Matches['name']
        $value = $Matches['value']
        if ($name -match '(?i)patreon' -or $value -match '(?i)patreon') {
          $csResults += [pscustomobject]@{
            ConstantName = $name
            Code         = $value
            File         = $file.FullName.Substring($RepoRoot.Length).TrimStart('\', '/')
            Line         = $lineNum
          }
        }
      }
    }
  }

$csResults = $csResults | Sort-Object Code, File -Unique

Write-Host "`n=== Patreon-related permission constants found in code ===" -ForegroundColor Yellow
$csResults | Format-Table ConstantName, Code, File, Line -AutoSize

$csCsvPath = Join-Path $OutputDir "patreon-permissions-found.csv"
$csResults | Export-Csv -Path $csCsvPath -NoTypeInformation
Write-Host "Written: $csCsvPath" -ForegroundColor Green

$uniqueCodes = $csResults.Code | Sort-Object -Unique

if ($uniqueCodes.Count -eq 0) {
  Write-Warning "No Patreon-related permission constants found under $RepoRoot. Check -RepoRoot points at the src/ directory."
}

# ── Pass 2: historical references in *.sql files (permission codes + 'Patreon' role) ──

$sqlResults = @()

Get-ChildItem -Path $RepoRoot -Recurse -Include *.sql -File |
  ForEach-Object {
    $file = $_
    $lineNum = 0
    Get-Content -LiteralPath $file.FullName | ForEach-Object {
      $lineNum++
      $line = $_
      if ($line -match '(?i)patreon') {
        $sqlResults += [pscustomobject]@{
          File    = $file.FullName.Substring($RepoRoot.Length).TrimStart('\', '/')
          Line    = $lineNum
          Content = $line.Trim()
        }
      }
    }
  }

Write-Host "`n=== Historical Patreon references in SQL files ===" -ForegroundColor Yellow
$sqlResults | Format-Table File, Line, Content -AutoSize

$sqlCsvPath = Join-Path $OutputDir "patreon-permission-history.csv"
$sqlResults | Export-Csv -Path $sqlCsvPath -NoTypeInformation
Write-Host "Written: $sqlCsvPath" -ForegroundColor Green

# ── Emit the DB gap-check SQL ────────────────────────────────────────────────
# Adjust table/schema names below if your current Administration schema uses
# different names than assumed here (administration.permissions,
# administration.role_permissions, administration.roles / user_roles).

$valuesClause = ($uniqueCodes | ForEach-Object { "    ('$_')" }) -join ",`n"

$sql = @"
-- =============================================================================
-- Patreon permission gap check — generated $(Get-Date -Format 'yyyy-MM-dd HH:mm')
-- Source codes discovered in *Auth.cs across the repo.
-- =============================================================================

-- 1. Which of these codes are missing from administration.permissions entirely?
WITH found_codes(code) AS (
  VALUES
$valuesClause
)
SELECT f.code AS missing_permission_code
FROM found_codes f
LEFT JOIN administration.permissions p ON p.code = f.code
WHERE p.code IS NULL;

-- 2. For codes that DO exist, which roles actually have them?
--    (Empty or missing 'Patreon'/'Administrator'/'SuperAdministrator' rows here
--    are the real gap even when the permission code itself exists.)
WITH found_codes(code) AS (
  VALUES
$valuesClause
)
SELECT f.code, rp.role_name
FROM found_codes f
LEFT JOIN administration.role_permissions rp ON rp.permission_code = f.code
ORDER BY f.code, rp.role_name;

-- 3. Does the 'Patreon' role exist at all post-migration?
--    (users.roles/.role_permissions/.permissions were dropped in
--    20260420175252_Drop_Users_Access_Controls — confirm the role survived
--    the cutover to the administration schema.)
SELECT * FROM administration.roles WHERE name = 'Patreon';

-- 4. Does any user currently hold the 'Patreon' role?
SELECT * FROM administration.user_roles WHERE role_name = 'Patreon';
"@

$sqlOutPath = Join-Path $OutputDir "patreon-permissions-gap-check.sql"
$sql | Out-File -FilePath $sqlOutPath -Encoding utf8
Write-Host "`nWritten: $sqlOutPath — run this in DBeaver against the dev database." -ForegroundColor Green

Write-Host "`nDone. $($csResults.Count) constant(s) found across $($uniqueCodes.Count) unique code(s)." -ForegroundColor Cyan
