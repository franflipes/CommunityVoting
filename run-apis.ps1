# Run all .NET APIs with dotnet run (no Docker image build).
# Prerequisites: .\start-infra.ps1
# Usage:
#   .\run-apis.ps1
#   .\run-apis.ps1 -NoNewWindows   # same terminal, background jobs

param(
    [switch]$NoNewWindows
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot

$apis = @(
    @{
        Name = "main-api"
        Project = "src\CommunityVoting.API\CommunityVoting.API.csproj"
        Url = "http://localhost:5004"
    },
    @{
        Name = "voting-api"
        Project = "src\CommunityVoting.Voting.Api\CommunityVoting.Voting.Api.csproj"
        Url = "http://localhost:5222"
    },
    @{
        Name = "document-api"
        Project = "src\CommunityVoting.Document.Api\CommunityVoting.Document.Api.csproj"
        Url = "http://localhost:5088"
    }
)

foreach ($api in $apis) {
    $projectPath = Join-Path $Root $api.Project
    if (-not (Test-Path $projectPath)) {
        Write-Error "Project not found: $projectPath"
    }
}

Write-Host "Starting APIs with dotnet run (infra must be running: .\start-infra.ps1)" -ForegroundColor Green
Write-Host ""

if ($NoNewWindows) {
    foreach ($api in $apis) {
        $projectPath = Join-Path $Root $api.Project
        Write-Host "Starting $($api.Name) -> $($api.Url)" -ForegroundColor Cyan
        Start-Job -Name $api.Name -ScriptBlock {
            param($Path)
            Set-Location (Split-Path $Path -Parent)
            dotnet run --project $Path --launch-profile http
        } -ArgumentList $projectPath | Out-Null
    }
    Write-Host ""
    Write-Host "APIs started as background jobs. Logs:  Get-Job | Receive-Job" -ForegroundColor Yellow
    Write-Host "Stop jobs:  Get-Job | Stop-Job; Get-Job | Remove-Job" -ForegroundColor Yellow
} else {
    foreach ($api in $apis) {
        $projectPath = Join-Path $Root $api.Project
        $projectDir = Split-Path $projectPath -Parent
        Write-Host "Opening $($api.Name) -> $($api.Url)" -ForegroundColor Cyan
        Start-Process powershell -ArgumentList @(
            "-NoExit",
            "-Command",
            "Set-Location '$projectDir'; Write-Host '$($api.Name) on $($api.Url)' -ForegroundColor Green; dotnet run --launch-profile http"
        )
        Start-Sleep -Seconds 2
    }
    Write-Host ""
    Write-Host "Three PowerShell windows opened. Close them to stop the APIs." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Frontend (separate terminal):  cd frontend; npm run dev" -ForegroundColor Yellow
