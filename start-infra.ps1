# Start infrastructure containers only (no .NET build, no npm).
# Usage:
#   .\start-infra.ps1
#   .\start-infra.ps1 -WithPgAdmin

param(
    [switch]$WithPgAdmin
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot

if (-not (Get-Command podman -ErrorAction SilentlyContinue)) {
    Write-Error "podman not found in PATH."
}

Set-Location $Root

$profiles = @()
if ($WithPgAdmin) { $profiles += "tools" }

if ($profiles.Count -gt 0) {
    $profileArgs = ($profiles | ForEach-Object { "--profile", $_ }) -join " "
    Write-Host "podman compose $profileArgs up -d" -ForegroundColor Cyan
    podman compose @($profiles | ForEach-Object { "--profile"; $_ }) up -d
} else {
    Write-Host "podman compose up -d" -ForegroundColor Cyan
    podman compose up -d
}

Write-Host ""
Write-Host "Infrastructure is up:" -ForegroundColor Green
Write-Host "  PostgreSQL  127.0.0.1:5433  (postgres / postgres / communityvoting)"
Write-Host "  Azurite     blob 10000, queue 10001, table 10002"
if ($WithPgAdmin) {
    Write-Host "  pgAdmin     http://127.0.0.1:5050  (admin@admin.com / root)"
    Write-Host "  Note: pgAdmin may take 1-3 minutes to become ready."
}
Write-Host ""
Write-Host "Run APIs locally:  .\run-apis.ps1" -ForegroundColor Yellow
Write-Host "Run frontend:      cd frontend; npm run dev" -ForegroundColor Yellow
