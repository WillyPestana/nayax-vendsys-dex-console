#!/usr/bin/env bash
set -euo pipefail

BACKUP_PATH="/var/opt/mssql/backup/VendSysDex.bak"
LOCAL_PATH="database/backups/VendSysDex.bak"
PASSWORD="${MSSQL_SA_PASSWORD:-YourStrong!Passw0rd}"

docker compose exec -T sqlserver bash -lc "mkdir -p /var/opt/mssql/backup && SQLCMD=\$(command -v sqlcmd || command -v /opt/mssql-tools18/bin/sqlcmd || command -v /opt/mssql-tools/bin/sqlcmd) && \"\$SQLCMD\" -S localhost -U sa -P '$PASSWORD' -C -Q \"BACKUP DATABASE [VendSysDex] TO DISK = N'$BACKUP_PATH' WITH INIT, FORMAT\""
docker compose cp "sqlserver:$BACKUP_PATH" "$LOCAL_PATH"

echo "Backup written to $LOCAL_PATH"
