# Generate-Dto.ps1
# Fetches the OpenAPI spec from the running API and regenerates dto.ts via NSwag.
# Run from C:\Repos\ScreenDrafts

param(
  [string]$ApiBase = "http://localhost:5000",
  [string]$SpecPath = "./docs/screendrafts-api.json",
  [string]$OutputPath = "./src/screendrafts.ui/src/lib/dto.ts"
)

$specUrl = "$ApiBase/openapi/v1.json"

Write-Host "Fetching OpenAPI spec from $specUrl..." -ForegroundColor Cyan

try {
  Invoke-WebRequest -Uri $specUrl -OutFile $SpecPath -ErrorAction Stop
  Write-Host "Saved to $SpecPath" -ForegroundColor Green
}
catch {
  Write-Error "Failed to fetch spec. Is the API running at $ApiBase?"
  exit 1
}

Write-Host "Generating dto.ts..." -ForegroundColor Cyan

nswag openapi2tsclient `
  /input:$SpecPath `
  /output:$OutputPath `
  /template:Fetch `
  /typeStyle:Interface `
  /generateClientClasses:true `
  /generateClientInterfaces:true `
  /useAbortSignal:true

if ($LASTEXITCODE -ne 0) {
  Write-Error "NSwag generation failed."
  exit 1
}

Write-Host "Done. $OutputPath updated." -ForegroundColor Green