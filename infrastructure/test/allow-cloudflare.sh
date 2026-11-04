#!/bin/bash
# infrastructure/test/allow-cloudflare.sh
#
# Lets Cloudflare's edge servers reach Caddy on ports 80 and 443.
# With the orange cloud on, every visitor, and Let's Encrypt's certificate
# check, arrives from a Cloudflare address. If ufw admits only your home IP,
# Cloudflare times out and shows error 522.
#
# Run as root on the server. Safe to repeat: ufw skips rules that exist.
# Cloudflare changes its ranges now and then; rerun to pick up new ones.
#
# Also add the same ranges to the Hetzner Cloud Firewall (inbound TCP 80, 443).
# The two firewalls are independent, and both must allow the traffic.

set -euo pipefail
[ "$(id -u)" -eq 0 ] || { echo "Run as root."; exit 1; }

tmp="$(mktemp)"
curl -fsS https://www.cloudflare.com/ips-v4 > "${tmp}"
echo >> "${tmp}"
curl -fsS https://www.cloudflare.com/ips-v6 >> "${tmp}"

count=0
while read -r range; do
  [ -z "${range}" ] && continue
  ufw allow from "${range}" to any port 80 proto tcp comment 'cloudflare'  >/dev/null
  ufw allow from "${range}" to any port 443 proto tcp comment 'cloudflare' >/dev/null
  count=$((count + 1))
done < "${tmp}"
rm -f "${tmp}"

echo "Added rules for ${count} Cloudflare ranges."
echo
echo "Hetzner Cloud Firewall: paste these into inbound TCP 80 and 443 rules:"
curl -fsS https://www.cloudflare.com/ips-v4
echo
curl -fsS https://www.cloudflare.com/ips-v6
echo
ufw status numbered | head -20
