#!/usr/bin/env bash
set -euo pipefail

COMPOSE_FILE="compose.prod.yaml"

usage() {
    echo "Usage: $0 <command>"
    echo ""
    echo "Commands:"
    echo "  update       Pull, rebuild and restart all services"
    echo "  import       Run a fresh database import"
    echo "  logs-web     Show logs for the web service"
    echo "  logs-worker  Show logs for the worker service"
    echo ""
}

cmd_update() {
    echo "==> Pulling latest changes..."
    git pull
    echo "==> Building and restarting services..."
    docker compose -f "$COMPOSE_FILE" up -d --build --remove-orphans
    echo "==> Cleaning up unused images and volumes..."
    docker system prune -a --volumes -f
    echo "==> Done."
}

cmd_import() {
    echo "==> Running import..."
    docker compose -f "$COMPOSE_FILE" --profile import run --rm --build --remove-orphans companyosint-import
    echo "==> Done."
}

cmd_logs() {
    local service="$1"
    docker compose -f "$COMPOSE_FILE" --profile worker logs -f "$service"
}

case "${1:-}" in
    update)      cmd_update ;;
    import)      cmd_import ;;
    logs-web)    cmd_logs companyosint-web ;;
    logs-worker) cmd_logs companyosint-worker ;;
    *)           usage; exit 1 ;;
esac
