<#
.SYNOPSIS
  Resets every password-credential Keycloak user in the dev realm to a random,
  strong password. Users who authenticate only via a federated identity
  (Google, Microsoft) are skipped automatically — they have no password
  credential to reset.

.NOTES
  Run this against DEV before exporting the realm for production import.
  Requires the realm admin username/password (KEYCLOAK_ADMIN /
  KEYCLOAK_ADMIN_PASSWORD from your dev docker-compose.yml).

.USAGE
  .\reset-dev-passwords.ps1 `
    -KeycloakUrl "http://localhost:18080" `
    -Realm "screendrafts" `
    -AdminUser "admin" `
    -AdminPassword "admin"
#>

param(
  [Parameter(Mandatory = $true)][string]$KeycloakUrl,
  [Parameter(Mandatory = $true)][string]$Realm,
  [Parameter(Mandatory = $true)][string]$AdminUser,
  [Parameter(Mandatory = $true)][string]$AdminPassword
)

function New-RandomPassword {
  $bytes = New-Object byte[] 24
  [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
  return [Convert]::ToBase64String($bytes)
}

Write-Host "Authenticating against master realm as $AdminUser..."
$tokenResponse = Invoke-RestMethod -Method Post `
  -Uri "$KeycloakUrl/realms/master/protocol/openid-connect/token" `
  -ContentType "application/x-www-form-urlencoded" `
  -Body @{
    grant_type = "password"
    client_id  = "admin-cli"
    username   = $AdminUser
    password   = $AdminPassword
  }

$adminToken = $tokenResponse.access_token
$headers = @{ Authorization = "Bearer $adminToken" }

Write-Host "Fetching all users in realm '$Realm'..."
$allUsers = @()
$first = 0
$pageSize = 100

do {
  $page = Invoke-RestMethod -Method Get `
    -Uri "$KeycloakUrl/admin/realms/$Realm/users?first=$first&max=$pageSize" `
    -Headers $headers
  $allUsers += $page
  $first += $pageSize
} while ($page.Count -eq $pageSize)

Write-Host "Found $($allUsers.Count) total users."

$resetCount = 0
$skippedCount = 0
$results = @()

foreach ($user in $allUsers) {
  $userId = $user.id
  $username = $user.username

  # Skip users who only have a federated (social) identity — they have no
  # password credential in Keycloak to reset.
  $federated = Invoke-RestMethod -Method Get `
    -Uri "$KeycloakUrl/admin/realms/$Realm/users/$userId/federated-identity" `
    -Headers $headers

  $credentials = Invoke-RestMethod -Method Get `
    -Uri "$KeycloakUrl/admin/realms/$Realm/users/$userId/credentials" `
    -Headers $headers

  $hasPasswordCredential = $credentials | Where-Object { $_.type -eq "password" }

  if ($federated.Count -gt 0 -and -not $hasPasswordCredential) {
    Write-Host "  SKIP  $username (social login only, no password credential)"
    $skippedCount++
    continue
  }

  $newPassword = New-RandomPassword

  Invoke-RestMethod -Method Put `
    -Uri "$KeycloakUrl/admin/realms/$Realm/users/$userId/reset-password" `
    -Headers $headers `
    -ContentType "application/json" `
    -Body (@{
      type      = "password"
      value     = $newPassword
      temporary = $true
    } | ConvertTo-Json)

  Write-Host "  RESET $username"
  $resetCount++
  $results += [pscustomobject]@{ Username = $username; UserId = $userId }
}

Write-Host ""
Write-Host "Done. $resetCount password(s) reset, $skippedCount social-only account(s) skipped."
Write-Host ""
Write-Host "Note: passwords were set to random values with 'temporary = true'."
Write-Host "No one can currently log in with them — that's intentional. Real"
Write-Host "users will set their own password via the email/password recovery"
Write-Host "flow (README Step 9) once that's built and prod is live."