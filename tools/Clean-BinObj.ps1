<#
.SYNOPSIS
  Recursively deletes all bin/ and obj/ folders under the given root.

.PARAMETER Root
  Repo root to search from. Defaults to the current directory.

.EXAMPLE
  .\Clean-BinObj.ps1
  .\Clean-BinObj.ps1 -Root C:\Repos\ScreenDrafts
#>

param(
  [string]$Root = (Get-Location)
)

$folders = Get-ChildItem -Path $Root -Directory -Recurse -Force -ErrorAction SilentlyContinue |
  Where-Object { $_.Name -in @('bin', 'obj') }

if (-not $folders) {
  Write-Host "No bin/obj folders found under $Root" -ForegroundColor Yellow
  return
}

Write-Host "Found $($folders.Count) folder(s) to delete under $Root" -ForegroundColor Cyan

foreach ($folder in $folders) {
  try {
    Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
    Write-Host "Deleted: $($folder.FullName)" -ForegroundColor Green
  }
  catch {
    Write-Warning "Failed to delete $($folder.FullName): $_"
  }
}

Write-Host "Done." -ForegroundColor Cyan