#!/bin/bash
# vps-setup.sh
#
# Run once on a fresh Hetzner Ubuntu 24.04 VPS as root.
# Sets up Docker, Tailscale, ufw firewall, and the /opt/screendrafts directory layout.
#
# Usage:
#   ssh root@<VPS_IP>
#   curl -fsSL https://raw.githubusercontent.com/hmsiegel/ScreenDrafts/main/infrastructure/vps-setup.sh | bash
#
# Or copy and run manually.

set -e

YOUR_HOME_IP="CHANGE_ME_your_home_ip"         # e.g. 1.2.3.4  — get it from whatismyip.com
FRIEND_IPS=()                                  # Add friend IPs: ("1.2.3.4" "5.6.7.8")
DEPLOY_USER="screendrafts"

echo "==> Updating system packages..."
apt-get update -qq && apt-get upgrade -y -qq

echo "==> Installing dependencies..."
apt-get install -y -qq \
  ca-certificates curl gnupg lsb-release ufw unattended-upgrades

# ── Docker ─────────────────────────────────────────────────────────────────
echo "==> Installing Docker..."
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | \
  gpg --dearmor -o /etc/apt/keyrings/docker.gpg
chmod a+r /etc/apt/keyrings/docker.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] \
  https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | \
  tee /etc/apt/sources.list.d/docker.list > /dev/null

apt-get update -qq
apt-get install -y -qq docker-ce docker-ce-cli containerd.io docker-compose-plugin

systemctl enable docker
systemctl start docker

# ── Deploy user ────────────────────────────────────────────────────────────
echo "==> Creating deploy user: ${DEPLOY_USER}..."
useradd -m -s /bin/bash "${DEPLOY_USER}" || echo "User already exists."
usermod -aG docker "${DEPLOY_USER}"

# ── Directory layout ───────────────────────────────────────────────────────
echo "==> Creating /opt/screendrafts layout..."
mkdir -p /opt/screendrafts/{secrets,keycloak/{import,providers},logs}
chown -R "${DEPLOY_USER}:${DEPLOY_USER}" /opt/screendrafts
chmod 700 /opt/screendrafts/secrets

# ── Tailscale ──────────────────────────────────────────────────────────────
echo "==> Installing Tailscale..."
curl -fsSL https://tailscale.com/install.sh | sh
echo "==> Run 'tailscale up' after this script completes to authenticate."

# ── Firewall (ufw) ─────────────────────────────────────────────────────────
echo "==> Configuring firewall..."
ufw default deny incoming
ufw default allow outgoing

# SSH — your IP only
ufw allow from "${YOUR_HOME_IP}" to any port 22 proto tcp

# HTTP/HTTPS — your IP
ufw allow from "${YOUR_HOME_IP}" to any port 80 proto tcp
ufw allow from "${YOUR_HOME_IP}" to any port 443 proto tcp
ufw allow from "${YOUR_HOME_IP}" to any port 443 proto udp

# Friends
for ip in "${FRIEND_IPS[@]}"; do
  ufw allow from "${ip}" to any port 80 proto tcp
  ufw allow from "${ip}" to any port 443 proto tcp
  ufw allow from "${ip}" to any port 443 proto udp
done

# Tailscale UDP
ufw allow 41641/udp

ufw --force enable
ufw status verbose

# ── Automatic security updates ─────────────────────────────────────────────
echo "==> Enabling unattended security upgrades..."
dpkg-reconfigure --priority=low unattended-upgrades

echo ""
echo "==> VPS setup complete."
echo ""
echo "Next steps:"
echo "  1. Run: tailscale up"
echo "  2. Copy docker-compose.production.yml and infrastructure/ to /opt/screendrafts/"
echo "  3. Run: /opt/screendrafts/infrastructure/secrets/provision-secrets.sh"
echo "  4. Copy Keycloak realm export + JARs to /opt/screendrafts/keycloak/"
echo "  5. Add GitHub Actions secrets: VPS_HOST, VPS_USER, VPS_SSH_KEY, GHCR_TOKEN"
echo "  6. Push to main to trigger first deploy."
