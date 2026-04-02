#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COMPOSE_FILE="$ROOT_DIR/database/postgres/docker-compose.yml"

echo "Starting PostgreSQL (docker compose)..."
docker compose -f "$COMPOSE_FILE" up -d

echo "Waiting for PostgreSQL healthcheck..."
for i in {1..60}; do
  if docker exec clti-postgres pg_isready -U postgres -d postgres >/dev/null 2>&1; then
    echo "PostgreSQL is ready."
    exit 0
  fi
  sleep 1
done

echo "PostgreSQL did not become ready in time." >&2
exit 1
