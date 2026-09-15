# PowerShell script to manage and run CommunityVoting APIs
param (
    [Parameter(Position=0, Mandatory=$false)]
    [string]$Target
)

$apiMainPath     = "src/CommunityVoting.API/CommunityVoting.API.csproj"
$apiDocumentPath = "src/CommunityVoting.Document.Api/CommunityVoting.Document.Api.csproj"
$apiVotingPath   = "src/CommunityVoting.Voting.Api/CommunityVoting.Voting.Api.csproj"
$appHostPath     = "src/CommunityVoting.AppHost/CommunityVoting.AppHost.csproj"

function Get-PortStatus([int]$port) {
    try {
        $tcp = New-Object System.Net.Sockets.TcpClient
        $asyncResult = $tcp.BeginConnect("127.0.0.1", $port, $null, $null)
        $wait = $asyncResult.AsyncWaitHandle.WaitOne(200, $false)
        if ($wait -and $tcp.Connected) {
            $tcp.Close()
            return $true
        }
        if ($tcp.Connected) { $tcp.Close() }
        return $false
    } catch {
        return $false
    }
}

function Print-StatusLine([string]$name, [int]$port, [string]$url) {
    $isOnline = Get-PortStatus -port $port
    Write-Host "  * $name (Puerto $port): " -NoNewline
    if ($isOnline) {
        Write-Host "[ ONLINE  ] " -ForegroundColor Green -NoNewline
    } else {
        Write-Host "[ OFFLINE ] " -ForegroundColor Red -NoNewline
    }
    Write-Host "-> $url" -ForegroundColor DarkGray
}

function Show-Menu {
    Write-Host "`n========================================================" -ForegroundColor Cyan
    Write-Host "         COMMUNITY VOTING - ESTADO Y CONTROL DE APIs" -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Cyan
    Write-Host " ESTADO ACTUAL DE LAS APIs:" -ForegroundColor White
    
    Print-StatusLine "Main API    " 5004 "http://localhost:5004"
    Print-StatusLine "Document API" 5088 "http://localhost:5088"
    Print-StatusLine "Voting API  " 5222 "http://localhost:5222"
    
    Write-Host "--------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host " OPCIONES PARA INICIAR (En ventana independiente):" -ForegroundColor White
    Write-Host "  [1] Iniciar Main API           (Port 5004)" -ForegroundColor Yellow
    Write-Host "  [2] Iniciar Document API       (Port 5088)" -ForegroundColor Yellow
    Write-Host "  [3] Iniciar Voting API         (Port 5222)" -ForegroundColor Yellow
    Write-Host "  [4] Iniciar TODAS las 3 APIs   (3 ventanas separadas)" -ForegroundColor Magenta
    Write-Host "  [5] Iniciar .NET Aspire        (AppHost)" -ForegroundColor Cyan
    Write-Host "--------------------------------------------------------" -ForegroundColor DarkGray
    Write-Host "  [R] Refrescar estado" -ForegroundColor Gray
    Write-Host "  [Q] Salir" -ForegroundColor DarkGray
    Write-Host "========================================================`n" -ForegroundColor Cyan
}

function Show-StatusOnly {
    Write-Host "`n========================================================" -ForegroundColor Cyan
    Write-Host "         ESTADO ACTUAL DE LAS APIs DE COMMUNITY VOTING" -ForegroundColor Green
    Write-Host "========================================================" -ForegroundColor Cyan
    Print-StatusLine "Main API    " 5004 "http://localhost:5004"
    Print-StatusLine "Document API" 5088 "http://localhost:5088"
    Print-StatusLine "Voting API  " 5222 "http://localhost:5222"
    Write-Host "========================================================`n" -ForegroundColor Cyan
}

function Start-ApiInNewWindow([string]$title, [string]$projectPath, [int]$port) {
    Write-Host "`n[+] Lanzando $title en una nueva ventana de PowerShell..." -ForegroundColor Green
    Write-Host "[*] URL: http://localhost:$port" -ForegroundColor Yellow
    
    $command = "Write-Host '========================================================' -ForegroundColor Cyan; " +
               "Write-Host ' SERVICIO: $title' -ForegroundColor Green; " +
               "Write-Host ' PUERTO: $port' -ForegroundColor Yellow; " +
               "Write-Host '========================================================' -ForegroundColor Cyan; " +
               "dotnet run --project $projectPath"
               
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $command
}

function Process-Choice([string]$choice) {
    switch ($choice.ToString().ToLower()) {
        { $_ -in "1", "main", "api" } {
            Start-ApiInNewWindow "Main API (CommunityVoting.API)" $apiMainPath 5004
            return $true
        }
        { $_ -in "2", "doc", "document", "documentapi" } {
            Start-ApiInNewWindow "Document API (CommunityVoting.Document.Api)" $apiDocumentPath 5088
            return $true
        }
        { $_ -in "3", "vote", "voting", "votingapi" } {
            Start-ApiInNewWindow "Voting API (CommunityVoting.Voting.Api)" $apiVotingPath 5222
            return $true
        }
        { $_ -in "4", "all", "todas" } {
            Start-ApiInNewWindow "Main API (CommunityVoting.API)" $apiMainPath 5004
            Start-ApiInNewWindow "Document API (CommunityVoting.Document.Api)" $apiDocumentPath 5088
            Start-ApiInNewWindow "Voting API (CommunityVoting.Voting.Api)" $apiVotingPath 5222
            return $true
        }
        { $_ -in "5", "aspire", "apphost" } {
            Start-ApiInNewWindow ".NET Aspire (AppHost)" $appHostPath 18888
            return $true
        }
        { $_ -in "status", "estado" } {
            Show-StatusOnly
            return $false
        }
        { $_ -in "r", "refrescar", "refresh" } {
            return $true
        }
        { $_ -in "q", "exit", "salir" } {
            Write-Host "Operacion finalizada." -ForegroundColor DarkGray
            exit 0
        }
        default {
            Write-Host "`n[ERROR] Opcion no valida '$choice'." -ForegroundColor Red
            return $true
        }
    }
}

if (-not [string]::IsNullOrWhiteSpace($Target)) {
    $shouldContinue = Process-Choice $Target
    if (-not $shouldContinue) { exit 0 }
}

while ($true) {
    Show-Menu
    $inputChoice = Read-Host "Elige una opcion (1-5, R para refrescar, Q para salir)"
    $null = Process-Choice $inputChoice
}
