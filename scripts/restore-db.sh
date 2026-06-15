#!/usr/bin/env bash
set -euo pipefail

LOCAL_PATH="${1:-database/backups/VendSysDex.bak}"
BACKUP_PATH="/var/opt/mssql/backup/VendSysDex.bak"
PASSWORD="${MSSQL_SA_PASSWORD:-YourStrong!Passw0rd}"

if [[ ! -f "$LOCAL_PATH" ]]; then
  echo "Backup file not found: $LOCAL_PATH" >&2
  exit 1
fi

docker compose exec -T sqlserver mkdir -p /var/opt/mssql/backup
docker compose cp "$LOCAL_PATH" "sqlserver:$BACKUP_PATH"
docker compose exec -T sqlserver bash -lc "SQLCMD=\$(command -v sqlcmd || command -v /opt/mssql-tools18/bin/sqlcmd || command -v /opt/mssql-tools/bin/sqlcmd) && \"\$SQLCMD\" -S localhost -U sa -P '$PASSWORD' -C -Q \"ALTER DATABASE [VendSysDex] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [VendSysDex] FROM DISK = N'$BACKUP_PATH' WITH REPLACE; ALTER DATABASE [VendSysDex] SET MULTI_USER;\""

echo "Database restored from $LOCAL_PATH"
