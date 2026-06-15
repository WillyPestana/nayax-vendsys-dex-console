#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if [[ -f "$ROOT_DIR/.env" ]]; then
  set -a
  # shellcheck disable=SC1091
  source "$ROOT_DIR/.env"
  set +a
fi

command -v dotnet >/dev/null || { echo "dotnet SDK 9 is required." >&2; exit 1; }
command -v npm >/dev/null || { echo "Node.js/npm is required." >&2; exit 1; }

API_ORIGIN="${API_ORIGIN:-http://localhost:8080}"
FRONTEND_ORIGIN="${FRONTEND_ORIGIN:-http://localhost:5173}"

export ASPNETCORE_URLS="${ASPNETCORE_URLS:-$API_ORIGIN}"
export Swagger__Enabled="${Swagger__Enabled:-true}"
export DatabaseInitializer__RunOnStartup="${DatabaseInitializer__RunOnStartup:-true}"
export DatabaseInitializer__ScriptsPath="${DatabaseInitializer__ScriptsPath:-$ROOT_DIR/database/scripts}"
export BasicAuth__Username="${BasicAuth__Username:-vendsys}"
export BasicAuth__Password="${BasicAuth__Password:-NFsZGmHAGWJSZ#RuvdiV}"
export Cors__AllowedOrigins__0="${Cors__AllowedOrigins__0:-$FRONTEND_ORIGIN}"
export Cors__AllowedOrigins__1="${Cors__AllowedOrigins__1:-http://localhost:3000}"
export VITE_API_BASE_URL="${VITE_API_BASE_URL:-$API_ORIGIN}"
export VITE_SIGNALR_HUB_URL="${VITE_SIGNALR_HUB_URL:-$API_ORIGIN/hubs/dex-processing}"

if [[ -z "${Persistence__ConnectionString:-}" ]]; then
  export Persistence__ConnectionString="Server=localhost,1433;Database=VendSysDex;User Id=sa;Password=${MSSQL_SA_PASSWORD:-YourStrong!Passw0rd};TrustServerCertificate=True;Encrypt=False"
fi

cleanup() {
  if [[ -n "${API_PID:-}" ]]; then kill "$API_PID" 2>/dev/null || true; fi
  if [[ -n "${FRONTEND_PID:-}" ]]; then kill "$FRONTEND_PID" 2>/dev/null || true; fi
}
trap cleanup EXIT INT TERM

if [[ ! -d "$ROOT_DIR/src/frontend/node_modules" ]]; then
  (cd "$ROOT_DIR/src/frontend" && npm install)
fi

echo "Starting API on $ASPNETCORE_URLS"
(cd "$ROOT_DIR" && dotnet run --project src/backend/src/NayaxVendSys.Api) &
API_PID=$!

echo "Starting frontend on $FRONTEND_ORIGIN"
(cd "$ROOT_DIR/src/frontend" && npm run dev -- --host 0.0.0.0) &
FRONTEND_PID=$!

echo
echo "Frontend: $FRONTEND_ORIGIN"
echo "Swagger:  $API_ORIGIN/swagger/"
echo "Health:   $API_ORIGIN/health"
echo
echo "Press Ctrl+C to stop both processes."

while kill -0 "$API_PID" 2>/dev/null && kill -0 "$FRONTEND_PID" 2>/dev/null; do
  sleep 1
done
