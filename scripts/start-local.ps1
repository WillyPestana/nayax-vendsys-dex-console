param(
    [string]$ConnectionString,
    [switch]$SkipInstall
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..")

$envFile = Join-Path $Root ".env"
if (Test-Path $envFile) {
    Get-Content $envFile | ForEach-Object {
        $line = $_.Trim()
        if ($line.Length -eq 0 -or $line.StartsWith("#") -or -not $line.Contains("=")) {
            return
        }

        $name, $value = $line.Split("=", 2)
        [Environment]::SetEnvironmentVariable($name.Trim(), $value.Trim(), "Process")
    }
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet SDK 9 is required."
}

if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    throw "Node.js/npm is required."
}

$apiOrigin = if ($env:API_ORIGIN) { $env:API_ORIGIN } else { "http://localhost:8080" }
$frontendOrigin = if ($env:FRONTEND_ORIGIN) { $env:FRONTEND_ORIGIN } else { "http://localhost:5173" }

if (-not $env:ASPNETCORE_URLS) { $env:ASPNETCORE_URLS = $apiOrigin }
if (-not $env:Swagger__Enabled) { $env:Swagger__Enabled = "true" }
if (-not $env:DatabaseInitializer__RunOnStartup) { $env:DatabaseInitializer__RunOnStartup = "true" }
if (-not $env:DatabaseInitializer__ScriptsPath) { $env:DatabaseInitializer__ScriptsPath = Join-Path $Root "database/scripts" }
if (-not $env:BasicAuth__Username) { $env:BasicAuth__Username = "vendsys" }
if (-not $env:BasicAuth__Password) { $env:BasicAuth__Password = "NFsZGmHAGWJSZ#RuvdiV" }
if (-not $env:Cors__AllowedOrigins__0) { $env:Cors__AllowedOrigins__0 = $frontendOrigin }
if (-not $env:Cors__AllowedOrigins__1) { $env:Cors__AllowedOrigins__1 = "http://localhost:3000" }
if (-not $env:VITE_API_BASE_URL) { $env:VITE_API_BASE_URL = $apiOrigin }
if (-not $env:VITE_SIGNALR_HUB_URL) { $env:VITE_SIGNALR_HUB_URL = "$apiOrigin/hubs/dex-processing" }

if ($ConnectionString) {
    $env:Persistence__ConnectionString = $ConnectionString
}
elseif (-not $env:Persistence__ConnectionString) {
    $isWindowsHost = ($PSVersionTable.PSEdition -eq "Desktop") -or $IsWindows
    if ($isWindowsHost -and (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) {
        $env:Persistence__ConnectionString = "Server=(localdb)\MSSQLLocalDB;Database=VendSysDex;Trusted_Connection=True;TrustServerCertificate=True;"
    }
    else {
        $password = if ($env:MSSQL_SA_PASSWORD) { $env:MSSQL_SA_PASSWORD } else { "YourStrong!Passw0rd" }
        $env:Persistence__ConnectionString = "Server=localhost,1433;Database=VendSysDex;User Id=sa;Password=$password;TrustServerCertificate=True;Encrypt=False"
    }
}

$frontendPath = Join-Path $Root "src/frontend"
if (-not $SkipInstall -and -not (Test-Path (Join-Path $frontendPath "node_modules"))) {
    Push-Location $frontendPath
    npm install
    Pop-Location
}

Write-Host "Starting API on $($env:ASPNETCORE_URLS)"
$api = Start-Process dotnet `
    -ArgumentList @("run", "--project", (Join-Path $Root "src/backend/src/NayaxVendSys.Api")) `
    -WorkingDirectory $Root `
    -PassThru `
    -NoNewWindow

Write-Host "Starting frontend on $frontendOrigin"
$frontend = Start-Process npm `
    -ArgumentList @("run", "dev", "--", "--host", "0.0.0.0") `
    -WorkingDirectory $frontendPath `
    -PassThru `
    -NoNewWindow

Write-Host ""
Write-Host "Frontend: $frontendOrigin"
Write-Host "Swagger:  $apiOrigin/swagger/"
Write-Host "Health:   $apiOrigin/health"
Write-Host ""
Write-Host "Press Ctrl+C to stop both processes."

try {
    while (-not $api.HasExited -and -not $frontend.HasExited) {
        Start-Sleep -Seconds 1
    }
}
finally {
    if (-not $api.HasExited) { Stop-Process -Id $api.Id -Force }
    if (-not $frontend.HasExited) { Stop-Process -Id $frontend.Id -Force }
}
