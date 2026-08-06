#!/bin/bash
# provision-secrets.sh
#
# Run this ONCE on the VPS to create all Docker secret files under /opt/screendrafts/secrets/.
# Fill in every value marked CHANGE_ME before running.
# After running: chmod 600 /opt/screendrafts/secrets/*.txt
#
# Usage:
#   chmod +x provision-secrets.sh
#   sudo ./provision-secrets.sh

set -e

SECRETS_DIR="/opt/screendrafts/secrets"
mkdir -p "${SECRETS_DIR}"

write_secret() {
  local name="$1"
  local value="$2"
  printf '%s' "${value}" > "${SECRETS_DIR}/${name}.txt"
}

# ── PostgreSQL superuser ───────────────────────────────────────────────────
write_secret "pg_superuser"     "screendrafts_admin"
write_secret "pg_superpassword" "CHANGE_ME_pg_superpassword"

# ── Per-module DB connection strings ──────────────────────────────────────
# Replace CHANGE_ME_* with the passwords you set when creating each DB role.
PG_HOST="screendrafts.database"
PG_DB="screendrafts"

write_secret "db_connection_string"          "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=screendrafts_admin;Password=CHANGE_ME_pg_superpassword"
write_secret "db_connection_administration"  "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=administration_user;Password=CHANGE_ME_administration_password"
write_secret "db_connection_audit"           "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=audit_user;Password=CHANGE_ME_audit_password"
write_secret "db_connection_communications"  "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=communications_user;Password=CHANGE_ME_communications_password"
write_secret "db_connection_drafts"          "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=drafts_user;Password=CHANGE_ME_drafts_password"
write_secret "db_connection_integrations"    "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=integrations_user;Password=CHANGE_ME_integrations_password"
write_secret "db_connection_movies"          "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=movies_user;Password=CHANGE_ME_movies_password"
write_secret "db_connection_realtimeupdates" "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=real_time_updates_user;Password=CHANGE_ME_realtimeupdates_password"
write_secret "db_connection_reporting"       "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=reporting_user;Password=CHANGE_ME_reporting_password"
write_secret "db_connection_users"           "Host=${PG_HOST};Port=5432;Database=${PG_DB};Username=users_user;Password=CHANGE_ME_users_password"

# ── Redis ──────────────────────────────────────────────────────────────────
REDIS_PASS="CHANGE_ME_redis_password"
write_secret "redis_password"          "${REDIS_PASS}"
write_secret "redis_connection_string" "screendrafts.redis:6379,password=${REDIS_PASS}"

# ── RabbitMQ ───────────────────────────────────────────────────────────────
RABBIT_USER="screendrafts"
RABBIT_PASS="CHANGE_ME_rabbitmq_password"
write_secret "rabbitmq_user"              "${RABBIT_USER}"
write_secret "rabbitmq_password"          "${RABBIT_PASS}"
write_secret "rabbitmq_connection_string" "amqp://${RABBIT_USER}:${RABBIT_PASS}@screendrafts-queue:5672"

# ── MongoDB ────────────────────────────────────────────────────────────────
MONGO_USER="screendrafts_mongo"
MONGO_PASS="CHANGE_ME_mongo_password"
write_secret "mongo_user"              "${MONGO_USER}"
write_secret "mongo_password"          "${MONGO_PASS}"
write_secret "mongo_connection_string" "mongodb://${MONGO_USER}:${MONGO_PASS}@screendrafts.mongo:27017"

# ── Keycloak ───────────────────────────────────────────────────────────────
write_secret "keycloak_db_user"                   "keycloak"
write_secret "keycloak_db_password"               "CHANGE_ME_keycloak_db_password"
write_secret "keycloak_admin_user"                "admin"
write_secret "keycloak_admin_password"            "CHANGE_ME_keycloak_admin_password"
write_secret "keycloak_confidential_client_secret" "CHANGE_ME_keycloak_confidential_client_secret"
write_secret "keycloak_registration_secret"       "CHANGE_ME_keycloak_registration_secret"

# ── External APIs (copy from dev vault-init.sh — these don't change) ──────
write_secret "mediatr_license_key"       "CHANGE_ME_mediatr_license_key"
write_secret "tmdb_access_token"         "CHANGE_ME_tmdb_access_token"
write_secret "imdb_key"                  "CHANGE_ME_imdb_key"
write_secret "omdb_key"                  "CHANGE_ME_omdb_key"
write_secret "igdb_client_id"            "CHANGE_ME_igdb_client_id"
write_secret "igdb_client_secret"        "CHANGE_ME_igdb_client_secret"
write_secret "zoom_account_id"           "CHANGE_ME_zoom_account_id"
write_secret "zoom_client_id"            "CHANGE_ME_zoom_client_id"
write_secret "zoom_client_secret"        "CHANGE_ME_zoom_client_secret"
write_secret "zoom_webhook_secret_token" "CHANGE_ME_zoom_webhook_secret_token"

# ── Hetzner Object Storage (backup) ───────────────────────────────────────
write_secret "backup_s3_key"    "CHANGE_ME_hetzner_s3_access_key"
write_secret "backup_s3_secret" "CHANGE_ME_hetzner_s3_secret_key"

# ── Lock down permissions ──────────────────────────────────────────────────
chmod 600 "${SECRETS_DIR}"/*.txt
chown root:root "${SECRETS_DIR}"/*.txt

echo "Secrets written to ${SECRETS_DIR}/"
echo "Review each CHANGE_ME value before starting the stack."
