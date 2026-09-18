# ScreenDrafts Production Deployment

## Architecture

| Component | Where |
|---|---|
| Next.js frontend | Vercel (free tier), root directory `src/screendrafts.ui` |
| .NET API | Hetzner CX23 VPS — Docker |
| PostgreSQL | Hetzner CX23 VPS — Docker |
| Redis | Hetzner CX23 VPS — Docker |
| RabbitMQ | Hetzner CX23 VPS — Docker |
| Keycloak | Hetzner CX23 VPS — Docker |
| MongoDB | Hetzner CX23 VPS — Docker |
| Caddy (reverse proxy + TLS) | Hetzner CX23 VPS — Docker |
| DNS + CDN | Cloudflare |
| Private access | Tailscale + ufw allowlist |

**Server:** Hetzner CX23 (2 vCPU / 4 GB RAM / 40 GB SSD, shared/cost-optimized), Falkenstein, Ubuntu 24.04, ~$6.49/month. No separate volume — root disk is sufficient for this workload. Shared CPU is correct for this traffic profile; dedicated CPU is unnecessary.

**Domain:** `screen-drafts.com` via Cloudflare Registrar (~$10/year). TLD/hyphen choice doesn't materially matter for a private beta.

---

## Repo layout

Files live in the app repo (`hmsiegel/ScreenDrafts`), checked in **except** `provision-secrets.sh` once populated with real values:

```
ScreenDrafts/
├── src/
│   ├── screendrafts.api/
│   │   └── ScreenDrafts.Web/
│   │       └── appsettings.Production.json      ← checked in, baked into Docker image
│   └── screendrafts.ui/                          ← Vercel root directory
├── tools/
├── infrastructure/
│   ├── compose/
│   │   └── docker-compose.production.yml         ← checked in
│   ├── caddy/
│   │   └── Caddyfile                              ← checked in
│   ├── backup/
│   │   └── backup.sh                              ← checked in
│   ├── secrets/
│   │   └── provision-secrets.sh                   ← NOT checked in once filled with real secrets
│   └── vps-setup.sh                               ← checked in
└── .github/
    └── workflows/
        ├── build.yml       (existing — path-filtered to backend changes only)
        └── deploy.yml      (new — builds images, deploys to VPS)
```

`appsettings.Production.json` never goes on the VPS directly — it's baked into the API Docker image at build time via GitHub Actions.

---

## Step 1 — Register the domain

1. [cloudflare.com/products/registrar](https://www.cloudflare.com/products/registrar/) → search `screen-drafts.com` → purchase.
2. Cloudflare is automatically authoritative DNS.

---

## Step 2 — Create the Hetzner VPS

1. [console.hetzner.cloud](https://console.hetzner.cloud/) → New Project → "screendrafts".
2. Add Server:
   - **Location:** Falkenstein
   - **Image:** Ubuntu 24.04
   - **Type:** CX23 (shared/cost-optimized, 2 vCPU / 4 GB RAM / 40 GB SSD)
   - **SSH key:** add your public key during creation
   - **Volume:** none needed
   - **Name:** anything (e.g. `screendrafts`) — cosmetic only
3. Note the public IPv4.

---

## Step 3 — Configure Cloudflare DNS

| Type | Name | Content | Proxy |
|---|---|---|---|
| A | `@` | `<VPS_IP>` | ✅ Proxied |
| A | `www` | `<VPS_IP>` | ✅ Proxied |
| A | `api` | `<VPS_IP>` | ✅ Proxied |
| A | `auth` | `<VPS_IP>` | ✅ Proxied |

- Network → WebSockets: **Enable**
- SSL/TLS mode: **Full (strict)**

---

## Step 4 — Connect to the VPS

**If your SSH key lives in 1Password:**
1. Open the SSH key item in 1Password → export the private key to a local file.
2. Open **PuTTYgen** → Load Private Key → select the exported file → Save Private Key → produces a `.ppk` file.
3. In **PuTTY**: Host = `root@<VPS_IP>` → Connection → SSH → Auth → Credentials → browse to the `.ppk` file → Open.

**To copy files to the VPS**, use **WinSCP** (recommended over `pscp` for anything beyond one file):
1. New Session: Host = `<VPS_IP>`, Username = `root`, Advanced → SSH → Authentication → same `.ppk` file.
2. Drag and drop files/folders between the local and remote panes. WinSCP prompts to create missing remote directories.

---

## Step 5 — Run initial VPS setup

Edit `infrastructure/vps-setup.sh` locally before copying it over:
- Set `YOUR_HOME_IP` (find it at whatismyip.com)
- Add friend IPs to `FRIEND_IPS` as they're identified

Copy to `/root/` on the VPS via WinSCP, then in PuTTY:

```bash
chmod +x vps-setup.sh
bash vps-setup.sh
tailscale up
```

This installs Docker, creates the `screendrafts` deploy user, sets up `/opt/screendrafts/{secrets,keycloak/{import,providers},logs}`, installs Tailscale, and configures `ufw` (deny by default; allow your IP + friend IPs on 80/443; allow Tailscale UDP; SSH locked to your IP).

Invite beta testers to the Tailscale network via [tailscale.com/admin](https://tailscale.com/admin), or add their IP to `ufw` directly:
```bash
ufw allow from <FRIEND_IP> to any port 80 proto tcp
ufw allow from <FRIEND_IP> to any port 443 proto tcp
ufw allow from <FRIEND_IP> to any port 443 proto udp
```

---

## Step 6 — Copy deployment files to the VPS

Via WinSCP, recreate this structure under `/opt/screendrafts/`:

```
/opt/screendrafts/
├── docker-compose.production.yml
├── infrastructure/
│   ├── caddy/Caddyfile
│   ├── backup/backup.sh
│   └── secrets/provision-secrets.sh
└── keycloak/
    ├── import/       (realm export .json — see Step 9)
    └── providers/
        ├── keycloak-theme-screendrafts.jar
        └── screendrafts-authenticator-1.0.0.jar
```

---

## Step 7 — Provision secrets

Edit `provision-secrets.sh` on the VPS with `nano` (installed by default; `vim` can be installed via `apt-get install -y vim` if preferred):

```bash
nano /opt/screendrafts/infrastructure/secrets/provision-secrets.sh
```

For every `CHANGE_ME_*`:
- **Random secrets** (all DB role passwords, Redis, RabbitMQ, Mongo, Keycloak admin/db/client secrets, backup S3 keys): generate in 1Password (32-char, all character classes) and paste in.
- **External API keys** (MediatR license, TMDb, IMDb, OMDb, IGDB, Zoom): copy verbatim from dev's `vault-init.sh` — these are the same across environments.

This also currently needs a `guest_drafts_user` entry added (module exists in code but wasn't in the original secrets list — see Step 8).

Run it:
```bash
chmod +x /opt/screendrafts/infrastructure/secrets/provision-secrets.sh
sudo /opt/screendrafts/infrastructure/secrets/provision-secrets.sh
```

---

## Step 8 — Database: roles, schema grants, and data migration

Schema-per-module. Known schema names: `administration`, `audit`, `communications`, `drafts`, `guest_drafts`, `integrations`, `movies`, `real_time_updates`, `reporting`, `users`.

### 8a. Prep the dev database locally, before dumping

- Run the seeding project to purge and regenerate the `reporting` honorifics tables.
- Confirm whether Keycloak realm data is also being migrated (yes — see Step 9) so user rows in `users`/`administration` schemas will resolve against real Keycloak subject IDs post-migration.

### 8b. Dump dev database

```bash
pg_dump -h localhost -p 5432 -U postgres -d screendrafts -Fc -f screendrafts_dev.dump
```

Copy `screendrafts_dev.dump` to the VPS via WinSCP.

### 8c. Start the database only

```bash
cd /opt/screendrafts
docker compose -f docker-compose.production.yml up -d screendrafts.database
```

### 8d. Create login roles (no schema grants yet)

```bash
docker compose -f docker-compose.production.yml exec screendrafts.database psql -U screendrafts_admin -d screendrafts
```

```sql
CREATE ROLE administration_user WITH LOGIN PASSWORD 'paste_password';
CREATE ROLE audit_user WITH LOGIN PASSWORD 'paste_password';
CREATE ROLE communications_user WITH LOGIN PASSWORD 'paste_password';
CREATE ROLE drafts_user WITH LOGIN PASSWORD 'paste_password';
CREATE ROLE guest_drafts_user WITH LOGIN PASSWORD 'paste_password';
CREATE ROLE integrations_user WITH LOGIN PASSWORD 'paste_password';
CREATE ROLE movies_user WITH LOGIN PASSWORD 'paste_password';
CREATE ROLE real_time_updates_user WITH LOGIN PASSWORD 'paste_password';
CREATE ROLE reporting_user WITH LOGIN PASSWORD 'paste_password';
CREATE ROLE users_user WITH LOGIN PASSWORD 'paste_password';
```
`\q` to exit.

### 8e. Restore the dump directly (skip the migrator — the dump already has full schema + data)

```bash
docker cp screendrafts_dev.dump screendrafts-screendrafts.database-1:/tmp/
docker compose -f docker-compose.production.yml exec screendrafts.database \
  pg_restore -U screendrafts_admin -d screendrafts --no-owner --no-privileges /tmp/screendrafts_dev.dump
```

### 8f. Grant each role access to its own schema

```bash
docker compose -f docker-compose.production.yml exec screendrafts.database psql -U screendrafts_admin -d screendrafts
```

```sql
GRANT USAGE, CREATE ON SCHEMA administration TO administration_user;
GRANT USAGE, CREATE ON SCHEMA audit TO audit_user;
GRANT USAGE, CREATE ON SCHEMA communications TO communications_user;
GRANT USAGE, CREATE ON SCHEMA drafts TO drafts_user;
GRANT USAGE, CREATE ON SCHEMA guest_drafts TO guest_drafts_user;
GRANT USAGE, CREATE ON SCHEMA integrations TO integrations_user;
GRANT USAGE, CREATE ON SCHEMA movies TO movies_user;
GRANT USAGE, CREATE ON SCHEMA real_time_updates TO real_time_updates_user;
GRANT USAGE, CREATE ON SCHEMA reporting TO reporting_user;
GRANT USAGE, CREATE ON SCHEMA users TO users_user;

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA administration TO administration_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA audit TO audit_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA communications TO communications_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA drafts TO drafts_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA guest_drafts TO guest_drafts_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA integrations TO integrations_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA movies TO movies_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA real_time_updates TO real_time_updates_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA reporting TO reporting_user;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA users TO users_user;

ALTER DEFAULT PRIVILEGES IN SCHEMA administration GRANT ALL ON TABLES TO administration_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA audit GRANT ALL ON TABLES TO audit_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA communications GRANT ALL ON TABLES TO communications_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA drafts GRANT ALL ON TABLES TO drafts_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA guest_drafts GRANT ALL ON TABLES TO guest_drafts_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA integrations GRANT ALL ON TABLES TO integrations_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA movies GRANT ALL ON TABLES TO movies_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA real_time_updates GRANT ALL ON TABLES TO real_time_updates_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA reporting GRANT ALL ON TABLES TO reporting_user;
ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT ALL ON TABLES TO users_user;
```

The `ALTER DEFAULT PRIVILEGES` lines matter — without them, tables created by future migrations won't automatically grant the module role access.

> **Open item:** `guest_drafts` wasn't in the original `docker-compose.yml` / `vault-init.sh` — confirm its connection-string convention (own DB user vs. shared) and add `db_connection_guestdrafts` to both `provision-secrets.sh` and the compose file's `secrets:` list before first deploy.

---

## Step 9 — Keycloak realm migration + user password/email reset

Do this **before** Step 8's database role/grant work, so the realm export
reflects a clean, password-rotated state, and before restoring the dump —
the migrated realm's user `sub` claims need to line up with the `users`/
`administration` schema rows coming out of the same dev environment.

### 9a. Reset all dev passwords

Run `infrastructure/keycloak/reset-dev-passwords.ps1` against **dev**:

```powershell
.\reset-dev-passwords.ps1 `
  -KeycloakUrl "http://localhost:18080" `
  -Realm "screendrafts" `
  -AdminUser "admin" `
  -AdminPassword "admin"
```

Resets every password-credential user to a random, unknown value with
`temporary = true`. Accounts that only use Google/Microsoft login (no
Keycloak password credential) are detected automatically and skipped — no
action needed for social-only accounts. This just keeps known/weak test
passwords (e.g. `password`) out of the exported realm; real users set their
own password later via the recovery flow (9f), once prod is live.

### 9b. Export the realm

The Admin Console's "Partial export" **does not include users** — only
clients, roles, and groups. Since user identities must match the rows
already restored into Postgres, use the full CLI export instead, run from
inside the Keycloak container:

```bash
docker compose exec screendrafts.identity /opt/keycloak/bin/kc.sh export \
  --dir /tmp/export \
  --realm screendrafts \
  --users same_file

docker cp <keycloak_container_name>:/tmp/export ./keycloak-export
```

`--users same_file` includes user data (credential hashes/salts and
federated identity links) in the same JSON as the realm config. Copy the
result to `/opt/screendrafts/keycloak/import/` on the VPS —
`docker-compose.production.yml` already mounts this path with
`start --import-realm`.

### 9c. Rotate secrets for production (fresh values — do not reuse dev's)

| Secret | Where it's used |
|---|---|
| `screendrafts-confidential-client` secret | Keycloak client credentials tab; `keycloak_confidential_client_secret` in `provision-secrets.sh` |
| `SD_KEYCLOAK_SECRET` / `KeycloakRegistration__Secret` | Keycloak env + API env; `keycloak_registration_secret` in `provision-secrets.sh` |
| Google OAuth client secret | **New** production OAuth app in Google Cloud Console — dev's `localhost` redirect URI won't validate against `https://auth.screen-drafts.com/...` |
| Microsoft OAuth client secret | **New** production app registration in Azure AD — same redirect URI constraint |

Register redirect URIs for both providers as:
```
https://auth.screen-drafts.com/realms/screendrafts/broker/google/endpoint
https://auth.screen-drafts.com/realms/screendrafts/broker/microsoft/endpoint
```

Set the new values in the imported realm's identity provider config (Admin
Console → Identity Providers → google / microsoft → Client Secret) — the
imported realm JSON may still carry dev's values and needs correcting once
the realm is up, not just in `provision-secrets.sh`.

### 9d. Configure SMTP — Resend

1. Sign up at [resend.com](https://resend.com) (free tier: 3,000 emails/month, 100/day).
2. Add and verify `screen-drafts.com` — Resend gives you SPF/DKIM DNS records to add in Cloudflare.
3. Create a dedicated **Sending access** API key scoped to the domain (name it e.g. `keycloak-smtp-prod`) — don't use the default "Onboarding" key Resend creates at signup; that's for their own tutorial flow, not a production credential. Delete it once the dedicated key is confirmed working.
4. In Keycloak Admin Console → Realm Settings → Email:
   - **From:** `noreply@screen-drafts.com`
   - **Host:** `smtp.resend.com`
   - **Port:** `465`
   - **Encryption:** SSL
   - **Authentication:** enabled
   - **Username:** `resend`
   - **Password:** the Resend API key
5. Send a test email from that same settings page before relying on it for password resets.

### 9e. Copy theme + authenticator JARs

Already done — `keycloak-theme-screendrafts.jar` and
`screendrafts-authenticator-1.0.0.jar` are in place at
`/opt/screendrafts/keycloak/providers/`.

### 9f. Email/password recovery flow — built

Live in the Users module. Sequence for migrated users:

- **Bootstrap (fake → real email, one-time):** a signed, self-verifying token per user (`{userId, expiry}`, HMAC-SHA256, server secret — no DB storage). Distribute out-of-band (Discord, Patreon post, group text) since dev emails are known-fake. User visits `/email-change/claim?token=...`, enters their real email — no need to know the current fake one. Token expires in 72 hours. Disable or feature-flag this endpoint once the migration window closes; it's intentionally weaker-auth than the steady-state flow below and shouldn't stay live indefinitely.
- **Password reset trigger:** once the real email is confirmed, `PUT /admin/realms/{realm}/users/{id}/execute-actions-email` with `["UPDATE_PASSWORD"]` fires Keycloak's native reset email to the now-real address (`PUT`, not `POST` — previously fixed bug in `KeyCloakClient.SendPasswordResetEmailAsync`). Test this end-to-end against Resend (9d) before relying on it for real users — it was previously verified against Papercut in dev, but SMTP provider is a real variable.
- **Steady-state (real → real, ongoing):** standard double opt-in — request → confirmation link to current email → confirm → apply. Short-lived (1 hour), single-use, stored in the DB so it can be explicitly invalidated — unlike the bootstrap token.

**Before distributing bootstrap tokens to real users:** confirm the claim endpoint, the token expiry handling, and the `execute-actions-email` trigger all work end-to-end against prod Keycloak + Resend — not just against dev/Papercut.

---

## Step 10 — Add GitHub Actions secrets

Settings → Secrets and variables → Actions:

| Secret | Value |
|---|---|
| `VPS_HOST` | VPS public IP |
| `VPS_USER` | `screendrafts` |
| `VPS_SSH_KEY` | Private key for the deploy user |
| `GHCR_TOKEN` | GitHub PAT, `read:packages` scope |
| `BACKUP_S3_BUCKET` | Object storage bucket name |
| `BACKUP_S3_ENDPOINT` | Object storage endpoint URL |

`deploy.yml` triggers on successful completion of `Build`. `build.yml` should be path-filtered to `src/screendrafts.api/**` and `tools/**` so UI-only changes don't trigger a backend rebuild/redeploy.

---

## Step 11 — Set up Vercel

1. [vercel.com](https://vercel.com) → New Project → import `hmsiegel/ScreenDrafts`.
2. Root Directory: `src/screendrafts.ui`.
3. Environment variables:

| Key | Value |
|---|---|
| `NEXT_PUBLIC_API_URL` | `https://api.screen-drafts.com` |
| `NEXTAUTH_URL` | `https://www.screen-drafts.com` |
| `NEXTAUTH_SECRET` | `openssl rand -base64 32` |
| `KEYCLOAK_CLIENT_ID` | `screendrafts-public-client` |
| `KEYCLOAK_CLIENT_SECRET` | confidential client secret |
| `KEYCLOAK_ISSUER` | `https://auth.screen-drafts.com/realms/screendrafts` |

4. Deploy.

---

## Step 12 — First deploy

Push to `main`. `Build` runs → on success, `Deploy` builds/pushes Docker images to `ghcr.io/hmsiegel/screendrafts/` → SSHs into the VPS → `docker compose up -d`.

Manual trigger on the VPS if needed:
```bash
cd /opt/screendrafts
IMAGE_TAG=latest docker compose -f docker-compose.production.yml up -d
```

---

## Useful commands on VPS

```bash
# Status
docker compose -f /opt/screendrafts/docker-compose.production.yml ps

# Logs
docker compose -f /opt/screendrafts/docker-compose.production.yml logs -f screendrafts.web

# Manual backup
docker compose -f /opt/screendrafts/docker-compose.production.yml exec screendrafts.backup sh /backup.sh

# Restart one service
docker compose -f /opt/screendrafts/docker-compose.production.yml restart screendrafts.web
```

## Migrating to another provider later

The stack is plain Docker Compose — provider-agnostic. To move:
1. `pg_dump` on old server → copy dump to new server → restore
2. Copy `/opt/screendrafts/secrets/` to the new server
3. Run `vps-setup.sh` on the new server
4. Update `VPS_HOST` in GitHub Actions secrets
5. Update Cloudflare DNS A records to the new IP
6. Push to `main` to deploy

Expect ~15–30 minutes of downtime during cutover.