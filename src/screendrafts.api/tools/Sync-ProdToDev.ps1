# Sync-ProdToDev.ps1
# Run from: C:\Repos\ScreenDrafts\src\screendrafts.api
#
# Replaces the LOCAL dev database with a fresh copy of PROD. PROD is only ever READ
# (one pg_dump over SSH). Nothing here writes to PROD.
#
# Usage:
#   Full refresh:
#     .\tools\Sync-ProdToDev.ps1 -SshTarget screendrafts@<vps-tailscale-ip> -AdminEmail you@example.com
#
#   Backup only (e.g. right before running a fix in PROD):
#     .\tools\Sync-ProdToDev.ps1 -SshTarget screendrafts@<vps-tailscale-ip> -DumpOnly
#
#   Re-restore a dump you already have (no SSH):
#     .\tools\Sync-ProdToDev.ps1 -DumpFile .\.containers\sync\prod-20261029-153000.dump -AdminEmail you@example.com
#
#   Finish a refresh that stopped after the restore (e.g. a Keycloak login problem) without
#   restoring again - runs only the re-link, migrator and API restart:
#     .\tools\Sync-ProdToDev.ps1 -SkipRestore -AdminEmail you@example.com
#
# What a full refresh does:
#   1. pg_dump of the PROD 'screendrafts' database (custom format) streamed over SSH.
#   2. Drops and recreates the DEV 'screendrafts' database and restores the dump.
#   3. Runs Sync-ProdToDev.post-restore.sql: re-grants the per-module roles and marks every
#      unprocessed outbox/inbox message as processed, so DEV never replays PROD's queue.
#   4. Re-links -AdminEmail to its DEV Keycloak user (users.users.identity_id), so you can log in.
#   4b. Creates each module's login role (from your appsettings.Development.json connection
#      strings) and makes it own its schema's objects, via Sync-ProdToDev.roles.sql.
#   5. Runs the DbMigrator (on the host, via dotnet run) so DEV migrations that PROD doesn't have yet are applied.
#   6. Restarts the DEV API container so it drops connections to the old database.
#
# Requirements:
#   * Key-based SSH to the VPS as your admin user (in the docker group, e.g. hmsiegel). The key must
#     be loaded in ssh-agent: the script can't answer a passphrase prompt. Easiest is a ~\.ssh\config entry:
#         Host sd-vps
#           HostName <vps-tailscale-ip>
#           User hmsiegel
#           IdentityFile ~/.ssh/screendrafts_admin
#     then pass -SshTarget sd-vps.
#   * DEV Postgres must be the same major version as PROD (postgres:18). A PROD dump cannot be
#     restored into an older Postgres. If it fails with "unsupported version", change the image in
#     docker-compose.yml to postgres:18, delete .containers\db, and start the container again.
#   * DEV Keycloak must already have a user with -AdminEmail (create it once by hand).
#
# Not synced: MongoDB, Redis, RabbitMQ, Keycloak users/realm, R2 images.

param(
    [string]$SshTarget,
    [string]$ProdDbService = "screendrafts.database",
    [string]$DumpFile,
    [switch]$DumpOnly,
    [switch]$SkipRestore,
    [string]$AdminEmail,
    [string]$KeycloakUrl = "http://localhost:18080",
    [string]$Realm = "screendrafts",
    [string]$KeycloakAdminUser = "admin",
    [string]$KeycloakAdminPassword = "admin",
    [switch]$SkipRelink,
    [switch]$SkipMigrator,
    [switch]$Force
)

# Native commands (ssh, docker) signal failure through $LASTEXITCODE, not exceptions, and
# Windows PowerShell 5.1 turns redirected stderr into terminating errors under "Stop".
# So the default stays "Continue" and every native call is followed by Assert-Ok.
$ErrorActionPreference = "Continue"

function Assert-Ok {
    param([string]$What)
    if ($LASTEXITCODE -ne 0) { throw "$What failed (exit code $LASTEXITCODE)." }
}

function Write-Step {
    param([string]$Text)
    Write-Host ""
    Write-Host "==> $Text" -ForegroundColor Cyan
}

# ── Argument checks ──────────────────────────────────────────────────────────
if (-not $DumpFile -and -not $SshTarget -and -not $SkipRestore) {
    throw "Pass -SshTarget (to take a fresh dump), -DumpFile (to restore an existing one) or -SkipRestore."
}
if (-not $DumpOnly -and -not $SkipRelink -and -not $AdminEmail) {
    throw "Pass -AdminEmail (the account to re-link to DEV Keycloak) or -SkipRelink."
}

$syncDir = Join-Path (Get-Location) ".containers\sync"   # .containers is gitignored
New-Item -ItemType Directory -Force -Path $syncDir -ErrorAction Stop | Out-Null

# ── 1. Dump PROD (read-only) ─────────────────────────────────────────────────
if (-not $SkipRestore -and -not $DumpFile) {
    Write-Step "Dumping PROD over SSH ($SshTarget)"

    $DumpFile = Join-Path $syncDir ("prod-{0:yyyyMMdd-HHmmss}.dump" -f (Get-Date))
    $errFile = "$DumpFile.err"

    # The container is found by its compose label, so the compose file and its secret files
    # (which your admin user may not be able to read) are never touched.
    # The inner single quotes keep $(cat ...) for the container's shell: the credentials are
    # read from the Docker secrets inside the DB container and never leave the server.
    $remote = @'
docker exec $(docker ps -q --filter label=com.docker.compose.service={SVC}) sh -c 'PGPASSWORD=$(cat /run/secrets/pg_superpassword) pg_dump -U $(cat /run/secrets/pg_superuser) -d screendrafts --format=custom --no-owner --no-privileges'
'@
    $remote = $remote.Trim().Replace("{SVC}", $ProdDbService)

    # Start-Process hands ssh's stdout straight to the file as raw bytes. A PowerShell ">"
    # redirect would re-encode the binary dump as text and corrupt it.
    $p = Start-Process -FilePath "ssh" -ArgumentList @($SshTarget, "`"$remote`"") `
        -RedirectStandardOutput $DumpFile -RedirectStandardError $errFile `
        -NoNewWindow -Wait -PassThru
    if ($p.ExitCode -ne 0) {
        $err = ""
        if (Test-Path $errFile) { $err = Get-Content $errFile -Raw }
        throw "pg_dump over SSH failed (exit code $($p.ExitCode)).`n$err"
    }

    # A real custom-format dump starts with the bytes "PGDMP".
    $fs = [System.IO.File]::OpenRead($DumpFile)
    try {
        $head = New-Object byte[] 5
        [void]$fs.Read($head, 0, 5)
    }
    finally { $fs.Dispose() }
    if ([System.Text.Encoding]::ASCII.GetString($head) -ne "PGDMP") {
        throw "The dump file doesn't look like a Postgres custom-format dump. Check $errFile."
    }

    $sizeMb = [math]::Round((Get-Item $DumpFile).Length / 1MB, 1)
    Write-Host "Saved $DumpFile ($sizeMb MB)"
}
elseif (-not $SkipRestore -and -not (Test-Path $DumpFile)) {
    throw "Dump file not found: $DumpFile"
}

if ($DumpOnly) {
    Write-Host ""
    Write-Host "Backup only - nothing restored. Dump: $DumpFile" -ForegroundColor Green
    exit 0
}

# ── 2. Find the DEV database container ───────────────────────────────────────
Write-Step "Locating the DEV database container"

$devIds = @(docker ps --filter "label=com.docker.compose.service=screendrafts.database" -q | Where-Object { $_ })
Assert-Ok "docker ps"
if ($devIds.Count -ne 1) {
    throw "Expected exactly one running container for compose service 'screendrafts.database', found $($devIds.Count). Start it with: docker compose up -d screendrafts.database"
}
$dev = $devIds[0]
Write-Host ("DEV container: " + (docker inspect -f "{{.Name}} ({{.Config.Image}})" $dev))

if (-not $Force -and -not $SkipRestore) {
    $answer = Read-Host "This DROPS the local DEV database 'screendrafts' and replaces it with PROD data. Type YES to continue"
    if ($answer -ne "YES") { Write-Host "Cancelled."; exit 1 }
}

if (-not $SkipRestore) {
    # ── 3. Restore into DEV ──────────────────────────────────────────────────────
    Write-Step "Restoring into DEV"

    docker cp $DumpFile "${dev}:/tmp/prod.dump"
    Assert-Ok "docker cp (dump)"

    $listing = docker exec $dev pg_restore --list /tmp/prod.dump 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "DEV's pg_restore can't read the dump (usually: DEV Postgres is older than PROD's). Change the image to postgres:18 in docker-compose.yml, delete .containers\db, restart the container, then re-run.`n$listing"
    }

    docker exec $dev psql -U postgres -d postgres -v ON_ERROR_STOP=1 `
        -c "DROP DATABASE IF EXISTS screendrafts WITH (FORCE)" `
        -c "CREATE DATABASE screendrafts"
    Assert-Ok "drop/create DEV database"

    $restoreOut = docker exec $dev pg_restore -U postgres -d screendrafts --no-owner --no-privileges /tmp/prod.dump 2>&1 |
        ForEach-Object { "$_" }
    # "already exists" (e.g. the public schema) is expected on a fresh database; anything else is not.
    $restoreErrors = @($restoreOut | Where-Object { $_ -match "error:" -and $_ -notmatch "already exists" })
    if ($restoreErrors.Count -gt 0) {
        $restoreErrors | Select-Object -First 20 | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        throw "pg_restore reported $($restoreErrors.Count) error(s). DEV is in a partial state - fix the cause and re-run."
    }
    Write-Host "Restore finished."

    # ── 4. Post-restore fixes ────────────────────────────────────────────────────
    Write-Step "Re-granting module roles and neutralizing outbox/inbox"

    $postSql = Join-Path $PSScriptRoot "Sync-ProdToDev.post-restore.sql"
    if (-not (Test-Path $postSql)) { throw "Missing $postSql" }
    docker cp $postSql "${dev}:/tmp/post-restore.sql"
    Assert-Ok "docker cp (post-restore.sql)"
    docker exec $dev psql -U postgres -d screendrafts -v ON_ERROR_STOP=1 -f /tmp/post-restore.sql
    Assert-Ok "post-restore.sql"
}

# ── 5. Re-link the admin account to DEV Keycloak ─────────────────────────────
if (-not $SkipRelink) {
    Write-Step "Re-linking $AdminEmail to DEV Keycloak"

    # A failed Keycloak call must stop the script here. Without try/catch, PowerShell would carry
    # on to the next statement and report a misleading "found 0 users".
    try {
        $token = Invoke-RestMethod -Method Post -ErrorAction Stop `
            -Uri "$KeycloakUrl/realms/master/protocol/openid-connect/token" `
            -ContentType "application/x-www-form-urlencoded" `
            -Body @{
                grant_type = "password"
                client_id  = "admin-cli"
                username   = $KeycloakAdminUser
                password   = $KeycloakAdminPassword
            }
    }
    catch {
        throw "Couldn't log in to DEV Keycloak at $KeycloakUrl as '$KeycloakAdminUser'. The compose default (admin/admin) only applies the first time Keycloak starts on an empty database; if you've changed it, pass -KeycloakAdminPassword. Then re-run with -SkipRestore. Keycloak said: $($_.ErrorDetails.Message) ($($_.Exception.Message))"
    }
    $headers = @{ Authorization = "Bearer $($token.access_token)" }

    $emailParam = [uri]::EscapeDataString($AdminEmail)
    try {
        $kcUsers = @(Invoke-RestMethod -Method Get -ErrorAction Stop -Headers $headers `
            -Uri "$KeycloakUrl/admin/realms/$Realm/users?email=$emailParam&exact=true")
    }
    catch {
        throw "Keycloak user lookup failed in realm '$Realm': $($_.ErrorDetails.Message) ($($_.Exception.Message))"
    }
    if ($kcUsers.Count -ne 1) {
        throw "Found $($kcUsers.Count) DEV Keycloak users with email $AdminEmail in realm '$Realm'. Create exactly one, then re-run with -SkipRestore."
    }
    $kcId = $kcUsers[0].id

    # psql only interpolates :'vars' from stdin or -f, not from -c, hence the pipe.
    $update = "UPDATE users.users SET identity_id = :'kcid' WHERE lower(email) = lower(:'email');"
    $result = $update | docker exec -i $dev psql -U postgres -d screendrafts -v ON_ERROR_STOP=1 `
        -v "kcid=$kcId" -v "email=$AdminEmail"
    Assert-Ok "re-link UPDATE"
    if (("$result").Trim() -ne "UPDATE 1") {
        throw "Expected to re-link exactly 1 user, psql said: $result. Is $AdminEmail a user in PROD?"
    }
    Write-Host "Linked $AdminEmail -> Keycloak user $kcId"
}

# ── 5b. Module login roles and object ownership ──────────────────────────────
# A PROD dump carries no roles, and --no-owner leaves everything owned by postgres. The
# migrator and API log in as per-module roles (administration_user, ...), so each must exist
# with the password from your connection strings and own its schema. Idempotent; runs every time.
$migratorProjectDir = Join-Path $PSScriptRoot "ScreenDrafts.Tools.DbMigrator"
$devSettings = Join-Path $migratorProjectDir "appsettings.Development.json"
if (Test-Path $devSettings) {
    Write-Step "Creating module roles and fixing ownership"

    $rolesSql = Join-Path $PSScriptRoot "Sync-ProdToDev.roles.sql"
    if (-not (Test-Path $rolesSql)) { throw "Missing $rolesSql" }
    docker cp $rolesSql "${dev}:/tmp/roles.sql"
    Assert-Ok "docker cp (roles.sql)"

    # Connection-string key -> schema name (same list as the migrator's Program.cs).
    $schemaByKey = [ordered]@{
        Administration  = "administration"
        Audit           = "audit"
        Communications  = "communications"
        Drafts          = "drafts"
        GuestDrafts     = "guest_drafts"
        Integrations    = "integrations"
        Movies          = "movies"
        RealTimeUpdates = "real_time_updates"
        Reporting       = "reporting"
        Users           = "users"
    }
    $settings = Get-Content $devSettings -Raw | ConvertFrom-Json
    foreach ($key in $schemaByKey.Keys) {
        $cs = $settings.ConnectionStrings.$key
        if (-not $cs) { Write-Host "  $key : no connection string, skipped" -ForegroundColor Yellow; continue }

        $parts = @{}
        foreach ($seg in ($cs -split ";")) {
            $kv = $seg -split "=", 2
            if ($kv.Count -eq 2) { $parts[$kv[0].Trim().ToLower()] = $kv[1].Trim() }
        }
        $user = @("username", "user id", "userid", "uid", "user") | ForEach-Object { $parts[$_] } | Where-Object { $_ } | Select-Object -First 1
        $pass = @("password", "pwd") | ForEach-Object { $parts[$_] } | Where-Object { $_ } | Select-Object -First 1
        if (-not $user -or -not $pass) { Write-Host "  $key : no user/password in connection string, skipped" -ForegroundColor Yellow; continue }
        if ($user -eq "postgres") { continue }

        docker exec $dev psql -U postgres -d screendrafts -q -v ON_ERROR_STOP=1 `
            -v "rol=$user" -v "pw=$pass" -v "sch=$($schemaByKey[$key])" -f /tmp/roles.sql | Out-Null
        Assert-Ok "roles for $key"
        Write-Host "  $key : $user owns schema $($schemaByKey[$key])"
    }
}
else {
    Write-Host "No $devSettings - skipping module roles." -ForegroundColor Yellow
}

# ── 6. Apply DEV-only migrations ─────────────────────────────────────────────
if (-not $SkipMigrator) {
    Write-Step "Running the DbMigrator against DEV"

    # Runs on the host, like New-Migration.ps1. The compose 'screendrafts.migrator' container
    # has no connection strings (they live in your gitignored
    # tools\ScreenDrafts.Tools.DbMigrator\appsettings.Development.json and point at
    # localhost:5432), so inside a container it falls back to 127.0.0.1 and can't connect.
    $migratorProject = Join-Path $PSScriptRoot "ScreenDrafts.Tools.DbMigrator"
    $migratorSettings = Join-Path $migratorProject "appsettings.Development.json"
    if (-not (Test-Path $migratorSettings)) {
        throw "Missing $migratorSettings. The host-run migrator needs it for the DEV connection strings (it is gitignored). Create it, or re-run with -SkipMigrator and run the migrator from Visual Studio."
    }
    dotnet run --project $migratorProject
    Assert-Ok "DbMigrator"
}

# ── 7. Restart the API so it drops pooled connections to the old database ────
$webIds = @(docker ps --filter "label=com.docker.compose.service=screendrafts.web" -q | Where-Object { $_ })
foreach ($id in $webIds) {
    Write-Step "Restarting DEV API container"
    docker restart $id | Out-Null
    Assert-Ok "docker restart"
}

docker exec $dev rm -f /tmp/prod.dump /tmp/post-restore.sql /tmp/roles.sql | Out-Null

# ── Sanity check ─────────────────────────────────────────────────────────────
Write-Step "Row counts in DEV"
docker exec $dev psql -U postgres -d screendrafts -c `
    "SELECT (SELECT count(*) FROM drafts.drafts) AS drafts, (SELECT count(*) FROM drafts.draft_parts) AS parts, (SELECT count(*) FROM users.users) AS users"

Write-Host ""
Write-Host "DEV now mirrors PROD as of the dump." -ForegroundColor Green
if ($DumpFile) {
    Write-Host "Dump kept at: $DumpFile"
    Write-Host "It contains PROD data - delete it when you're done."
}
