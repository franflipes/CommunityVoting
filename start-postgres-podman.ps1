# PowerShell script to launch PostgreSQL and pgAdmin with Podman

Write-Host "Iniciando contenedores de PostgreSQL y pgAdmin con Podman..." -ForegroundColor Green

# 1. Check if podman CLI is installed
if (-not (Get-Command "podman" -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] El comando 'podman' no se encontró en el PATH. Asegúrate de tener Podman instalado." -ForegroundColor Red
    exit 1
}

# 2. Try podman compose / podman-compose up
$startedWithCompose = $false

if (Get-Command "podman-compose" -ErrorAction SilentlyContinue) {
    try {
        Write-Host "Ejecutando 'podman-compose up -d postgres pgadmin'..." -ForegroundColor Cyan
        podman-compose up -d postgres pgadmin
        $startedWithCompose = $true
    }
    catch {
        Write-Host "podman-compose falló, intentando comandos directos de podman run..." -ForegroundColor Yellow
    }
}

# 3. Fallback to direct podman run commands if podman-compose was not used
if (-not $startedWithCompose) {
    Write-Host "Desplegando contenedor PostgreSQL (postgres:17)..." -ForegroundColor Cyan
    podman run -d `
      --name communityvoting-postgres `
      -e POSTGRES_USER=postgres `
      -e POSTGRES_PASSWORD=postgres `
      -e POSTGRES_DB=communityvoting `
      -p 5433:5432 `
      -v communityvoting-postgres-data:/var/lib/postgresql/data `
      postgres:17

    Write-Host "Desplegando contenedor pgAdmin 4 (dpage/pgadmin4)..." -ForegroundColor Cyan
    podman run -d `
      --name communityvoting-pgadmin `
      -e PGADMIN_DEFAULT_EMAIL=admin@admin.com `
      -e PGADMIN_DEFAULT_PASSWORD=root `
      -e PGADMIN_LISTEN_PORT=80 `
      -p 5050:80 `
      -v communityvoting-pgadmin-data:/var/lib/pgadmin `
      dpage/pgadmin4:latest
}

Write-Host "`n========================================================" -ForegroundColor Green
Write-Host " Contenedores de PostgreSQL y pgAdmin iniciados en Podman " -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host " - PostgreSQL Database: 127.0.0.1:5433" -ForegroundColor Yellow
Write-Host "   - Database: communityvoting"
Write-Host "   - Username: postgres"
Write-Host "   - Password: postgres"
Write-Host " - pgAdmin 4 Dashboard: http://127.0.0.1:5050" -ForegroundColor Yellow
Write-Host "   - Email:    admin@admin.com"
Write-Host "   - Password: root"
Write-Host "========================================================`n"
