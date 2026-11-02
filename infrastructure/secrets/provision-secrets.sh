#!/bin/bash
# provision-secrets.sh  (replaces the old version; names match docker-compose.production.yml)
#
# Run ONCE on a NEW server (TEST) to create every Docker secret file under
# /opt/screendrafts/secrets/. It generates the passwords. It never overwrites
# an existing file, so a second run is safe and cannot clobber PROD secrets.
#
# Values the script cannot invent are written as CHANGE_ME. Fill them in, then
# check with:  grep -l CHANGE_ME /opt/screendrafts/secrets/*.txt
#
# Optional: put the external API values in /opt/screendrafts/external.env as
# KEY=value lines (names below) and the script uses them. Dev/test keys are fine
# for TMDB, IMDb, OMDb and IGDB. Use TEST Zoom credentials, not PROD's.
#
# Usage:
#   chmod +x provision-secrets.sh
#   sudo ./provision-secrets.sh

set -euo pipefail

BASE="/opt/screendrafts"
SECRETS_DIR="${BASE}/secrets"
EXTERNAL="${BASE}/external.env"
ENV_OUT="${BASE}/.env.generated"

mkdir -p "${SECRETS_DIR}"
chmod 755 "${SECRETS_DIR}"

# Pull in external values if present.
if [ -f "${EXTERNAL}" ]; then
  set -a
  # shellcheck disable=SC1090
  . "${EXTERNAL}"
  set +a
fi

# Hex passwords survive inside URI connection strings (no @ : / ? # characters).
gen() { openssl rand -hex 24; }

# write_secret NAME VALUE — skips a file that already exists.
write_secret() {
  local name="$1" value="$2" file="${SECRETS_DIR}/$1.txt"
  if [ -e "${file}" ]; then
    echo "keep   ${name}"
    return
  fi
  printf '%s' "${value}" > "${file}"
  echo "write  ${name}"
}

# read_secret NAME — print the value of a secret file (new or existing).
read_secret() { cat "${SECRETS_DIR}/$1.txt"; }

# ext VAR — value from external.env, else CHANGE_ME_VAR.
ext() {
  local var="$1"
  printf '%s' "${!var:-CHANGE_ME_${var}}"
}

PG_HOST="screendrafts.database"
PG_DB="screendrafts"

# ── PostgreSQL superuser ───────────────────────────────────────────────────
write_secret pg_superuser     "screendrafts_admin"
write_secret pg_superpassword "$(gen)"
PG_SUPER_PASS="$(read_secret pg_superpassword)"

# ── Per-module database roles and connection strings ──────────────────────
# Secret name suffix : role name.
# CHECK: guest_drafts_user is a guess; the old script had no GuestDrafts role.
# Compare these roles with whatever creates them (migrator or an init script).
ROLES=(
  "Administration:administration_user"
  "Audit:audit_user"
  "Communications:communications_user"
  "Drafts:drafts_user"
  "GuestDrafts:guest_drafts_user"
  "Integrations:integrations_user"
  "Movies:movies_user"
  "RealTimeUpdates:real_time_updates_user"
  "Reporting:reporting_user"
  "Users:users_user"
)

write_secret ConnectionStrings__Database \
  "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=screendrafts_admin;Password=${PG_SUPER_PASS}"

ROLE_REPORT=""
for entry in "${ROLES[@]}"; do
  module="${entry%%:*}"
  role="${entry##*:}"
  pass="$(gen)"
  write_secret "ConnectionStrings__${module}" \
    "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=${role};Password=${pass}"
  # Report what is actually on disk, even when the file already existed.
  actual="$(read_secret "ConnectionStrings__${module}" | sed -n 's/.*Password=\(.*\)$/\1/p')"
  ROLE_REPORT+="${role} ${actual}"$'\n'
done

# ── Redis ──────────────────────────────────────────────────────────────────
write_secret redis_password "$(gen)"
REDIS_PASS="$(read_secret redis_password)"
write_secret ConnectionStrings__Cache "screendrafts.redis:6379,password=${REDIS_PASS}"

# ── RabbitMQ ───────────────────────────────────────────────────────────────
write_secret rabbitmq_user     "screendrafts"
write_secret rabbitmq_password "$(gen)"
RABBIT_USER="$(read_secret rabbitmq_user)"
RABBIT_PASS="$(read_secret rabbitmq_password)"
write_secret ConnectionStrings__Queue "amqp://${RABBIT_USER}:${RABBIT_PASS}@screendrafts-queue:5672"
write_secret RabbitMq__UserName "${RABBIT_USER}"
write_secret RabbitMq__Password "${RABBIT_PASS}"

# ── MongoDB ────────────────────────────────────────────────────────────────
write_secret mongo_user     "screendrafts_mongo"
write_secret mongo_password "$(gen)"
MONGO_USER="$(read_secret mongo_user)"
MONGO_PASS="$(read_secret mongo_password)"
write_secret ConnectionStrings__Mongo "mongodb://${MONGO_USER}:${MONGO_PASS}@screendrafts.mongo:27017"

# ── Keycloak ───────────────────────────────────────────────────────────────
write_secret keycloak_db_user           "keycloak"
write_secret keycloak_db_password       "$(gen)"
write_secret keycloak_admin_user        "admin"
write_secret keycloak_admin_password    "$(gen)"
write_secret keycloak_registration_secret "$(gen)"
# These two come from the TEST realm. After the realm import, regenerate the
# client secrets in the Keycloak admin console and paste them here.
write_secret Audit__KeycloakPoller__ConfidentialClientSecret "CHANGE_ME_after_realm_import"
write_secret Users__KeyCloak__ConfidentialClientSecret       "CHANGE_ME_after_realm_import"

# ── Mail ───────────────────────────────────────────────────────────────────
# Mailpit accepts any login, so the password is a dummy.
write_secret Communications__Smtp__Password "mailpit"
write_secret Communications__OwnerNotification__Address "$(ext TEST_OWNER_EMAIL)"
# Cloudflare publishes always-pass test keys for Turnstile; use the TEST widget's secret.
write_secret Communications__Turnstile__SecretKey "$(ext TURNSTILE_SECRET_KEY)"

# ── External APIs (same keys as PROD are fine, except Zoom) ──────────────
write_secret MediatR__LicenseKey               "$(ext MEDIATR_LICENSE_KEY)"
write_secret Integrations__Tmdb__AccessToken   "$(ext TMDB_ACCESS_TOKEN)"
write_secret Integrations__Imdb__Key           "$(ext IMDB_KEY)"
write_secret Integrations__Omdb__Key           "$(ext OMDB_KEY)"
write_secret Integrations__Igdb__ClientId      "$(ext IGDB_CLIENT_ID)"
write_secret Integrations__Igdb__ClientSecret  "$(ext IGDB_CLIENT_SECRET)"
write_secret Integrations__Zoom__AccountId     "$(ext ZOOM_ACCOUNT_ID)"
write_secret Integrations__Zoom__ClientId      "$(ext ZOOM_CLIENT_ID)"
write_secret Integrations__Zoom__ClientSecret  "$(ext ZOOM_CLIENT_SECRET)"
write_secret Integrations__Zoom__WebhookSecretToken "$(ext ZOOM_WEBHOOK_SECRET_TOKEN)"
write_secret Integrations__Spotify__ClientId     "$(ext SPOTIFY_CLIENT_ID)"
write_secret Integrations__Spotify__ClientSecret "$(ext SPOTIFY_CLIENT_SECRET)"

# ── Bootstrap and app values ───────────────────────────────────────────────
write_secret Users__EmailBootstrap__Secret "$(gen)"
write_secret Shared__People__CommissionerPersonPublicIds__0 "$(ext COMMISSIONER_0_PUBLIC_ID)"
write_secret Shared__People__CommissionerPersonPublicIds__1 "$(ext COMMISSIONER_1_PUBLIC_ID)"
write_secret Shared__Predictions__ShootTheMoonIneligibleContestantIds__0 "$(ext SHOOT_THE_MOON_INELIGIBLE_ID)"

# ── Object storage ─────────────────────────────────────────────────────────
# R2: a TEST bucket with its own API token. Never reuse PROD's.
write_secret R2__AccessKeyId     "$(ext R2_ACCESS_KEY_ID)"
write_secret R2__SecretAccessKey "$(ext R2_SECRET_ACCESS_KEY)"
# Backups: a separate bucket or prefix from PROD's.
write_secret backup_s3_key    "$(ext BACKUP_S3_KEY)"
write_secret backup_s3_secret "$(ext BACKUP_S3_SECRET)"

# ── Permissions ────────────────────────────────────────────────────────────
# 644, not 600: the containers run as non-root users and read these files
# through the bind mount. The directory's 755 and the host's single-purpose
# role keep this acceptable. Tighten if you add other users to the box.
chmod 644 "${SECRETS_DIR}"/*.txt
chown root:root "${SECRETS_DIR}"/*.txt

# ── Values that must also appear in .env ───────────────────────────────────
# The compose file reads these from .env. They must match the secrets above.
umask 077
cat > "${ENV_OUT}" <<EOF
# Merge into /opt/screendrafts/.env. Values match the secret files.
KEYCLOAK_DB_USER=$(read_secret keycloak_db_user)
KEYCLOAK_DB_PASSWORD=$(read_secret keycloak_db_password)
RABBITMQ_DEFAULT_USER=${RABBIT_USER}
RABBITMQ_DEFAULT_PASS=${RABBIT_PASS}
KEYCLOAK_REGISTRATION_SECRET=$(read_secret keycloak_registration_secret)
EOF

echo
echo "── Create these roles in PostgreSQL (skip if the migrator does it) ──"
printf '%s' "${ROLE_REPORT}"
echo
echo "── Still to fill in ──"
grep -l CHANGE_ME "${SECRETS_DIR}"/*.txt || echo "(none)"
echo
echo "Merge ${ENV_OUT} into ${BASE}/.env, add the other .env values, then delete ${ENV_OUT}."
