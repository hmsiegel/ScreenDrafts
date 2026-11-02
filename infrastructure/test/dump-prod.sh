#!/bin/bash
# infrastructure/test/dump-prod.sh
#
# Run on PROD. Writes a compressed dump of the screendrafts database.
# Read-only against the database; it changes nothing on PROD.
#
# Usage:  ./dump-prod.sh
# Then copy the dump to TEST over Tailscale (command printed at the end).

set -euo pipefail
cd /opt/screendrafts

OUT="/tmp/screendrafts-$(date +%F).dump"
umask 077

docker compose -f docker-compose.production.yml exec -T screendrafts.database \
  sh -c 'pg_dump -U "$(cat /run/secrets/pg_superuser)" -d screendrafts -Fc --no-owner --no-privileges' \
  > "${OUT}"

ls -lh "${OUT}"
echo
echo "Copy it to TEST (from your PC or from this server):"
echo "  scp ${OUT} <user>@<test-tailscale-ip>:/tmp/"
echo "Then delete it here:  shred -u ${OUT}"
echo "The dump holds real emails until TEST scrubs it. Keep it off any shared drive."
