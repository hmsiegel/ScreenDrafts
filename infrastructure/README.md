# ScreenDrafts Production Deployment

## Architecture

| Component | Where |
|---|---|
| Next.js frontend | Vercel (free tier) |
| .NET API | Hetzner CX32 VPS — Docker |
| PostgreSQL | Hetzner CX32 VPS — Docker |
| Redis | Hetzner CX32 VPS — Docker |
| RabbitMQ | Hetzner CX32 VPS — Docker |
| Keycloak | Hetzner CX32 VPS — Docker |
| MongoDB | Hetzner CX32 VPS — Docker |
| Caddy (reverse proxy + TLS) | Hetzner CX32 VPS — Docker |
| DNS + CDN | Cloudflare |
| Private access | Tailscale |

---

## Step 1 — Register the domain

1. Go to [cloudflare.com/products/registrar](https://www.cloudflare.com/products/registrar/).
2. Search for `screen-drafts.com` and purchase it (~$10/year).
3. Cloudflare automatically becomes the authoritative DNS — no additional NS changes needed.

---

## Step 2 — Create the Hetzner VPS

1. Log in to [console.hetzner.cloud](https://console.hetzner.cloud/).
2. New Project → "screendrafts".
3. Add Server:
   - **Location:** Ashburn, VA (closest to US users) or Falkenstein (cheaper)
   - **Image:** Ubuntu 24.04
   - **Type:** CX32 (4 vCPU / 8 GB RAM)
   - **SSH key:** add your public key
   - **Firewall:** create a new one (you will configure it via `vps-setup.sh`)
4. Note the VPS public IP address.

---

## Step 3 — Configure Cloudflare DNS

In the Cloudflare dashboard for `screen-drafts.com`:

| Type | Name | Content | Proxy |
|---|---|---|---|
| A | `@` | `<VPS_IP>` | ✅ Proxied |
| A | `www` | `<VPS_IP>` | ✅ Proxied |
| A | `api` | `<VPS_IP>` | ✅ Proxied |
| A | `auth` | `<VPS_IP>` | ✅ Proxied |

In Cloudflare → Network → WebSockets: **Enable**.

SSL/TLS mode: set to **Full (strict)**.

---

## Step 4 — Set up the VPS

SSH in as root:

```bash
ssh root@<VPS_IP>
```

Edit `infrastructure/vps-setup.sh`:
- Set `YOUR_HOME_IP` to your public IP (`whatismyip.com`)
- Add any friend IPs to `FRIEND_IPS`

Run it:

```bash
bash infrastructure/vps-setup.sh
```

Then authenticate Tailscale:

```bash
tailscale up
```

Share the Tailscale node with beta testers via the Tailscale admin console (tailscale.com/admin).

---

## Step 5 — Provision secrets on the VPS

Edit `infrastructure/secrets/provision-secrets.sh`:
- Replace every `CHANGE_ME_*` value with real production credentials.
- Copy the API keys from your dev `vault-init.sh` (TMDb, IMDb, OMDb, IGDB, Zoom, MediatR).
- Generate strong passwords for all database users (`openssl rand -base64 32`).

Run it:

```bash
sudo bash infrastructure/secrets/provision-secrets.sh
```

---

## Step 6 — Copy deployment files to VPS

From your local machine:

```bash
scp compose/docker-compose.production.yml root@<VPS_IP>:/opt/screendrafts/
scp compose/appsettings.Production.json root@<VPS_IP>:/opt/screendrafts/
scp infrastructure/caddy/Caddyfile root@<VPS_IP>:/opt/screendrafts/infrastructure/caddy/
scp infrastructure/backup/backup.sh root@<VPS_IP>:/opt/screendrafts/infrastructure/backup/
```

Copy Keycloak files:

```bash
# Realm export (from your dev Keycloak — export via Admin UI or CLI)
scp .files/<your-realm-export>.json root@<VPS_IP>:/opt/screendrafts/keycloak/import/

# Theme + authenticator JARs
scp res/keycloak-theme-screendrafts.jar root@<VPS_IP>:/opt/screendrafts/keycloak/providers/
scp res/screendrafts-authenticator-1.0.0.jar root@<VPS_IP>:/opt/screendrafts/keycloak/providers/
```

---

## Step 7 — Add GitHub Actions secrets

In GitHub → Settings → Secrets and variables → Actions:

| Secret | Value |
|---|---|
| `VPS_HOST` | VPS public IP |
| `VPS_USER` | `screendrafts` (the deploy user created by setup script) |
| `VPS_SSH_KEY` | Private SSH key for the deploy user |
| `GHCR_TOKEN` | GitHub Personal Access Token with `read:packages` scope |
| `BACKUP_S3_BUCKET` | Hetzner Object Storage bucket name |
| `BACKUP_S3_ENDPOINT` | e.g. `https://fsn1.your-objectstorage.com` |

---

## Step 8 — Set up Vercel (Next.js frontend)

1. Go to [vercel.com](https://vercel.com) → New Project → Import `hmsiegel/ScreenDrafts`.
2. Set **Root Directory** to `src/screendrafts.ui`.
3. Add environment variables:

| Key | Value |
|---|---|
| `NEXT_PUBLIC_API_URL` | `https://api.screen-drafts.com` |
| `NEXTAUTH_URL` | `https://www.screen-drafts.com` |
| `NEXTAUTH_SECRET` | Generate: `openssl rand -base64 32` |
| `KEYCLOAK_CLIENT_ID` | `screendrafts-public-client` |
| `KEYCLOAK_CLIENT_SECRET` | Your confidential client secret |
| `KEYCLOAK_ISSUER` | `https://auth.screen-drafts.com/realms/screendrafts` |

4. Deploy.

---

## Step 9 — First deploy

Push to `main`. The `Build` workflow runs first; if it passes, `Deploy` triggers automatically, builds and pushes Docker images to `ghcr.io/hmsiegel/screendrafts/`, SSHs into the VPS, and runs `docker compose up -d`.

To trigger manually:

```bash
# On the VPS
cd /opt/screendrafts
IMAGE_TAG=latest docker compose -f docker-compose.production.yml up -d
```

---

## Adding a beta tester

**Option A — Firewall allowlist:**
```bash
# On VPS
ufw allow from <FRIEND_IP> to any port 80 proto tcp
ufw allow from <FRIEND_IP> to any port 443 proto tcp
ufw allow from <FRIEND_IP> to any port 443 proto udp
```

**Option B — Tailscale:**
Invite them via tailscale.com/admin → Users → Invite. They install Tailscale and access the site via its Tailscale IP (not the public domain).

---

## Useful commands on VPS

```bash
# View running containers
docker compose -f /opt/screendrafts/docker-compose.production.yml ps

# Tail API logs
docker compose -f /opt/screendrafts/docker-compose.production.yml logs -f screendrafts.web

# Run backup manually
docker compose -f /opt/screendrafts/docker-compose.production.yml exec screendrafts.backup sh /backup.sh

# Restart a single service
docker compose -f /opt/screendrafts/docker-compose.production.yml restart screendrafts.web
```
