
#!/bin/sh
# vault-init.sh
#
# Seeds all ScreenDrafts secrets into the Vault KV v2 engine.
# Runs as an init container (vault-init service in docker-compose.yml).
# Vault must be reachable and unsealed before this script executes —
# the init container depends_on screendrafts.vault with a health check.
#
# To re-seed after changing a value, run:
#   docker compose run --rm vault-init

set -e

export VAULT_ADDR="http://screendrafts.vault:8200"
export VAULT_TOKEN="${VAULT_DEV_ROOT_TOKEN_ID:-screendrafts-dev-root-token}"

echo "Waiting for Vault to be ready..."
until vault status > /dev/null 2>&1; do
  sleep 1
done

echo "Enabling KV v2 secrets engine at 'secret/'..."
vault secrets enable -path=secret kv-v2 2>/dev/null || echo "KV engine already enabled, continuing."

# ---------------------------------------------------------------------------
# Connection strings
# Each key maps to ConnectionStrings:<Key> in IConfiguration after the
# VaultSharp provider flattens "screendrafts/connectionstrings" into the
# configuration system.
# ---------------------------------------------------------------------------
echo "Writing secret/screendrafts/connectionstrings..."
vault kv put secret/screendrafts/connectionstrings \
  Database="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=postgres;Password=postgres;Include Error Detail=true" \
  Administration="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=administration_user;Password=administration_password;Include Error Detail=true" \
  Audit="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=audit_user;Password=audit_password;Include Error Detail=true" \
  Communications="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=communications_user;Password=communications_password;Include Error Detail=true" \
  Drafts="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=drafts_user;Password=drafts_password;Include Error Detail=true" \
  Integrations="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=integrations_user;Password=integrations_password;Include Error Detail=true" \
  Movies="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=movies_user;Password=movies_password;Include Error Detail=true" \
  RealTimeUpdates="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=real_time_updates_user;Password=real_time_updates_password;Include Error Detail=true" \
  Reporting="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=reporting_user;Password=reporting_password;Include Error Detail=true" \
  Users="Host=screendrafts.database;Port=5432;Database=screendrafts;Username=users_user;Password=users_password;Include Error Detail=true" \
  Cache="screendrafts.redis:6379" \
  Queue="amqp://screendrafts-queue:5672" \
  Mongo="mongodb://admin:admin@screendrafts.mongo:27017"

echo "Writing secret/screendrafts/mediatr..."
vault kv put secret/screendrafts/mediatr \
  "MediatR:LicenseKey"="YOUR_MEDIATR_LICENSE_HERE" \


# ---------------------------------------------------------------------------
# Third-party API keys
# Maps to Audit:KeycloakPoller
# colon-delimited flattening in VaultSharp.Extensions.Configuration.
# ---------------------------------------------------------------------------
echo "Writing secret/screendrafts/audit..."
vault kv put secret/screendrafts/audit \
  "Audit:KeycloakPoller:AdminUrl"="http://screendrafts.identity:8080/admin/realms/screendrafts/" \
  "Audit:KeycloakPoller:TokenUrl"="http://screendrafts.identity:8080/realms/screendrafts/protocol/openid-connect/token" \
  "Audit:KeycloakPoller:ConfidentialClientId"="screendrafts-confidential-client" \
  "Audit:KeycloakPoller:ConfidentialClientSecret"="oRL4la55pi1uMlJMKSlg3hrhLfvKrZsg"

# ---------------------------------------------------------------------------
# Third-party API keys
# Maps to Drafts:People
# colon-delimited flattening in VaultSharp.Extensions.Configuration.
# ---------------------------------------------------------------------------
echo "Writing secret/screendrafts/drafts..."
vault kv put secret/screendrafts/drafts \
  "Shared:People:CommissionerPersonPublicIds:0"="pe_FssS7cwtd2b78SC" \
  "Shared:People:CommissionerPersonPublicIds:1"="pe_qxaLv3nTNTjZqtF"
# ---------------------------------------------------------------------------

# Third-party API keys
# Maps to Integrations
# colon-delimited flattening in VaultSharp.Extensions.Configuration.
# ---------------------------------------------------------------------------
echo "Writing secret/screendrafts/integrations..."
vault kv put secret/screendrafts/integrations \
  "Integrations:Imdb:Key"="YOUR_IMDB_KEY_HERE" \
  "Integrations:Omdb:Key"="YOUR_OMDB_KEY_HERE" \
  "Integrations:Tmdb:AccessToken"="YOUR_TMDB_ACCESS_TOKEN_HERE" \
  "Integrations:Igdb:ClientId"="YOUR_IGDB_CLIENT_ID_HERE" \
  "Integrations:Igdb:ClientSecret"="YOUR_IGDB_CLIENT_SECRET_HERE" \
  "Integrations:Zoom:AccountId"="YOUR_ZOOM_ACCOUNT_ID_HERE" \
  "Integrations:Zoom:ClientId"="YOUR_ZOOM_CLIEND_ID_HERE" \
  "Integrations:Zoom:ClientSecret"="YOUR_ZOOM_CLIENT_SECRET_HERE" \
  "Integrations:Zoom:WebhookSecretToken"="YOUR_ZOOM_WEBHOOK_SECRET_HERE"

# ---------------------------------------------------------------------------
# Keycloak credentials
# Maps to Users:KeyCloak:* in IConfiguration (consumed by the Users module).
# ---------------------------------------------------------------------------
echo "Writing secret/screendrafts/keycloak..."
vault kv put secret/screendrafts/keycloak \
  "Users:KeyCloak:AdminUrl"="http://screendrafts.identity:8080/admin/realms/screendrafts/" \
  "Users:KeyCloak:TokenUrl"="http://screendrafts.identity:8080/realms/screendrafts/protocol/openid-connect/token" \
  "Users:KeyCloak:ConfidentialClientId"="screendrafts-confidential-client" \
  "Users:KeyCloak:ConfidentialClientSecret"="oRL4la55pi1uMlJMKSlg3hrhLfvKrZsg" \
  "Users:KeyCloak:PublicClientId"="screendrafts-public-client"

echo "Vault seeding complete."