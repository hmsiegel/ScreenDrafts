#!/bin/bash
# infrastructure/test/restore-test.sh
#
# Run on TEST only. Loads a PROD dump, creates the module roles, grants access
# and scrubs emails. Safe to repeat: each run replaces the previous data.
#
# Needs:  /opt/screendrafts/secrets/ConnectionStrings__*.txt (provision-secrets.sh)
#         scrub.sql in /opt/screendrafts/infrastructure/test/
#
# Usage:  sudo ./restore-test.sh /tmp/screendrafts-YYYY-MM-DD.dump

set -euo pipefail

BASE="/opt/screendrafts"
cd "${BASE}"
DUMP="${1:?usage: restore-test.sh /path/to/screendrafts.dump}"
[ -f "${DUMP}" ] || { echo "No such file: ${DUMP}"; exit 1; }

# Guard: only a TEST server sets the test compose profile. Never run this on PROD.
if ! grep -qE '^COMPOSE_PROFILES=.*test' .env; then
  echo "Refusing to run: COMPOSE_PROFILES=test is not set in ${BASE}/.env."
  exit 1
fi

COMPOSE="docker compose -f docker-compose.production.yml"
SU="$(cat secrets/pg_superuser.txt)"
psql_su() { ${COMPOSE} exec -T screendrafts.database psql -U "${SU}" -d screendrafts -v ON_ERROR_STOP=1 "$@"; }

# Stop the app so nothing writes while the data is replaced.
${COMPOSE} stop screendrafts.web || true
${COMPOSE} up -d --no-deps screendrafts.database
echo -n "Waiting for PostgreSQL"
until ${COMPOSE} exec -T screendrafts.database pg_isready -U "${SU}" -d screendrafts >/dev/null 2>&1; do
  echo -n "."; sleep 2
done
echo

# Module secret name : schema. Role name and password come from the secret file,
# so the roles always match what the API connects with.
MODULES=(
  "Administration:administration"
  "Audit:audit"
  "Communications:communications"
  "Drafts:drafts"
  "GuestDrafts:guest_drafts"
  "Integrations:integrations"
  "Movies:movies"
  "RealTimeUpdates:real_time_updates"
  "Reporting:reporting"
  "Users:users"
)

echo "Creating roles"
for entry in "${MODULES[@]}"; do
  module="${entry%%:*}"
  cs="$(cat "secrets/ConnectionStrings__${module}.txt")"
  role="$(sed -n 's/.*Username=\([^;]*\).*/\1/p' <<<"${cs}")"
  pass="$(sed -n 's/.*Password=\([^;]*\).*/\1/p' <<<"${cs}")"
  psql_su -q <<SQL
DO \$\$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '${role}') THEN
    CREATE ROLE ${role} WITH LOGIN PASSWORD '${pass}';
  ELSE
    ALTER ROLE ${role} WITH LOGIN PASSWORD '${pass}';
  END IF;
END
\$\$;
SQL
done

echo "Restoring ${DUMP}"
CID="$(${COMPOSE} ps -q screendrafts.database)"
docker cp "${DUMP}" "${CID}:/tmp/restore.dump"
${COMPOSE} exec -T screendrafts.database \
  pg_restore -U "${SU}" -d screendrafts --clean --if-exists --no-owner --no-privileges /tmp/restore.dump
${COMPOSE} exec -T screendrafts.database rm -f /tmp/restore.dump

echo "Granting schema access"
for entry in "${MODULES[@]}"; do
  module="${entry%%:*}"
  schema="${entry##*:}"
  cs="$(cat "secrets/ConnectionStrings__${module}.txt")"
  role="$(sed -n 's/.*Username=\([^;]*\).*/\1/p' <<<"${cs}")"
  psql_su -q <<SQL
GRANT USAGE, CREATE ON SCHEMA ${schema} TO ${role};
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA ${schema} TO ${role};
ALTER DEFAULT PRIVILEGES IN SCHEMA ${schema} GRANT ALL ON TABLES TO ${role};
SQL
done

echo "Scrubbing"
if ! psql_su < infrastructure/test/scrub.sql; then
  echo
  echo "SCRUB FAILED. The database holds real emails. Do not start the app."
  echo "Fix scrub.sql and run this script again."
  exit 1
fi

echo
echo "Restore finished. Next:"
echo "  1. Delete the dump:  shred -u ${DUMP}"
echo "  2. Apply migrations TEST's image has and PROD's lacks:"
echo "       ${COMPOSE} up --no-deps screendrafts.migrator"
echo "  3. Start the API:  ${COMPOSE} up -d --no-deps screendrafts.web"
