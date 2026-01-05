#!/usr/bin/env bash

set -euo pipefail

# --- Resolve absolute paths ---
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

COMPOSE_BASE="$REPO_ROOT/docker-compose.yml"
COMPOSE_SEEDING="$REPO_ROOT/docker-compose.seeding.yml"

# --- Default values ---
SEEDER=""
MODULE=""
REBUILD=false

# --- Argument parsing ---
while [[ $# -gt 0 ]]; do
  case $1 in
    movies|drafts|users)
      SEEDER=$1
      ;;
    --module=*)
      MODULE="${1#*=}"
      ;;
    --rebuild)
      REBUILD=true
      ;;
    *)
      echo "❌ Unknown argument: $1"
      echo "Usage: ./seed.sh [movies|drafts] [--module=name] [--rebuild]"
      exit 1
      ;;
  esac
  shift
done

if [[ -z "$SEEDER" ]]; then
  echo "❌ Seeder type must be 'movies' or 'drafts'."
  echo "Usage: ./seed.sh [movies|drafts] [--module=name] [--rebuild]"
  exit 1
fi

SEEDER_SERVICE="screendrafts.seeding.$SEEDER"

# --- Rebuild (optional) ---
if [[ "$REBUILD" = true ]]; then
  echo "🔧 Rebuilding $SEEDER_SERVICE..."
  docker compose -f "$COMPOSE_BASE" -f "$COMPOSE_SEEDING" build "$SEEDER_SERVICE"
fi

# --- Start DB ---
echo "🔄 Starting database container..."
docker compose -f "$COMPOSE_BASE" -f "$COMPOSE_SEEDING" up -d screendrafts.database

# --- Run Seeder ---
echo "🚀 Running $SEEDER_SERVICE $MODULE..."
docker compose -f "$COMPOSE_BASE" -f "$COMPOSE_SEEDING" run --rm "$SEEDER_SERVICE" \
  ${MODULE:+--module=$MODULE}

echo "✅ Seeding complete."

# --- Cleanup ---
echo "🧹 Cleaning up containers and volumes..."
docker compose -f "$COMPOSE_BASE" -f "$COMPOSE_SEEDING" down --volumes --remove-orphans

echo "✅ Done."