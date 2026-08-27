# PowerShell script to launch Azurite (Blobs & Queues) with Podman

Write-Host "Iniciando servicio de Storage Azurite (Blobs y Queues) en Podman..." -ForegroundColor Green

podman run -d `
  --name communityvoting-azurite `
  -p 10000:10000 `
  -p 10001:10001 `
  -p 10002:10002 `
  -v communityvoting-azurite-data:/data `
  mcr.microsoft.com/azure-storage/azurite `
  azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0 --location /data

Write-Host "Azurite iniciado correctamente." -ForegroundColor Green
Write-Host "Puertos mapeados:" -ForegroundColor Yellow
Write-Host " - Blob Storage:  127.0.0.1:10000"
Write-Host " - Queue Storage: 127.0.0.1:10001"
Write-Host " - Table Storage: 127.0.0.1:10002"
