# Smoke test for POST /stats/query. Run in PowerShell.
# Usage: .\stats-query-smoke-test.ps1 -BaseUrl "https://localhost:5001/api" -Token "<bearer token>"
param(
  [Parameter(Mandatory = $true)][string] $BaseUrl,
  [Parameter(Mandatory = $true)][string] $Token
)

$headers = @{ Authorization = "Bearer $Token" }

function Invoke-StatsQuery([string] $Title, [hashtable] $Body) {
  Write-Host "`n=== $Title ===" -ForegroundColor Cyan
  try {
    $json = $Body | ConvertTo-Json -Depth 5
    $result = Invoke-RestMethod -Method Post -Uri "$BaseUrl/stats/query" -Headers $headers `
      -ContentType "application/json" -Body $json
    Write-Host ("{0} by {1}: {2} groups{3}" -f $result.metricLabel, $result.groupBy, $result.totalGroups, $(if ($result.truncated) { " (truncated)" } else { "" }))
    $result.rows | Format-Table rank, name, value, context -AutoSize
  }
  catch {
    Write-Host ("HTTP {0}: {1}" -f $_.Exception.Response.StatusCode.value__, $_.ErrorDetails.Message) -ForegroundColor Yellow
  }
}

# Expect Ryan Marker first at 56
Invoke-StatsQuery "Vetoes used by drafter" @{ metric = "vetoesUsed"; groupBy = "drafter"; limit = 10 }

# Expect Ryan Marker 55 first
Invoke-StatsQuery "Appearances by drafter" @{ metric = "appearances"; groupBy = "drafter"; limit = 5 }

# Fewest vetoes per draft among drafters with 10+ drafts; ascending keeps zeros
Invoke-StatsQuery "Fewest vetoes per draft, 10+ drafts" @{ metric = "avgVetoesPerDraft"; groupBy = "drafter"; minAppearances = 10; ascending = $true; limit = 5 }

# How vetoes spread across series
Invoke-StatsQuery "Picks vetoed by series" @{ metric = "picksVetoed"; groupBy = "series" }

# Filters: Super drafts only, episodes 300 and up
Invoke-StatsQuery "Titles drafted by drafter, Super drafts, episode 300+" @{ metric = "titlesDrafted"; groupBy = "drafter"; draftTypes = @("Super"); episodeFrom = 300; limit = 10 }

# Expect HTTP 400: copacetic drafts cannot be grouped by title
Invoke-StatsQuery "Invalid pairing (expect 400)" @{ metric = "copaceticDrafts"; groupBy = "title" }

# Options as seen by this user
Write-Host "`n=== Options ===" -ForegroundColor Cyan
$options = Invoke-RestMethod -Uri "$BaseUrl/stats/query/options" -Headers $headers
Write-Host ("Series: {0}" -f ($options.series -join "; "))
Write-Host ("Draft types: {0}" -f ($options.draftTypes -join ", "))
Write-Host ("Episodes: {0} to {1}" -f $options.minEpisode, $options.maxEpisode)
