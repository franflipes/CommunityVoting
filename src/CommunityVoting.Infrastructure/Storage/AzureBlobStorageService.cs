using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommunityVoting.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CommunityVoting.Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private const string ContainerName = "community-documents";

    public AzureBlobStorageService(BlobServiceClient blobServiceClient, ILogger<AzureBlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    private async Task<BlobContainerClient> GetContainerClientAsync()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var created = await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        if (created != null)
        {
            _logger.LogInformation("[AZURE BLOB STORAGE] Contenedor '{ContainerName}' creado exitosamente en Azure Storage / Azurite.", ContainerName);
        }
        return containerClient;
    }

    public async Task<(string storagePath, long fileSize)> SaveFileAsync(Stream stream, string fileName, string folderName)
    {
        _logger.LogInformation("[AZURE BLOB STORAGE] 📤 Iniciando subida de archivo '{FileName}' (Carpeta: '{FolderName}') a Azure Storage / Azurite...", fileName, folderName);

        var containerClient = await GetContainerClientAsync();
        var blobName = $"{folderName}/{Guid.NewGuid()}_{Path.GetFileName(fileName)}".TrimStart('/');
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(stream, overwrite: true);
        var properties = await blobClient.GetPropertiesAsync();

        _logger.LogInformation("[AZURE BLOB STORAGE] ✅ Archivo '{FileName}' subido exitosamente a Azure Storage / Azurite. BlobPath: '{BlobName}', Tamaño: {FileSize} bytes.", fileName, blobName, properties.Value.ContentLength);

        return (blobName, properties.Value.ContentLength);
    }

    public async Task<(Stream stream, string contentType, string fileName)> GetFileAsync(string storagePath)
    {
        _logger.LogInformation("[AZURE BLOB STORAGE] 📥 Descargando archivo desde Azure Storage / Azurite. BlobPath: '{StoragePath}'...", storagePath);

        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(storagePath);

        if (!await blobClient.ExistsAsync())
        {
            _logger.LogWarning("[AZURE BLOB STORAGE] ⚠️ El archivo con BlobPath '{StoragePath}' no existe en Azure Storage.", storagePath);
            throw new FileNotFoundException("El archivo no existe en el almacenamiento Blob.", storagePath);
        }

        var downloadResult = await blobClient.DownloadStreamingAsync();
        var fileName = Path.GetFileName(storagePath);
        var contentType = downloadResult.Value.Details.ContentType ?? "application/octet-stream";

        _logger.LogInformation("[AZURE BLOB STORAGE] ✅ Archivo '{FileName}' descargado exitosamente de Azure Storage / Azurite.", fileName);

        return (downloadResult.Value.Content, contentType, fileName);
    }

    public async Task DeleteFileAsync(string storagePath)
    {
        _logger.LogInformation("[AZURE BLOB STORAGE] 🗑️ Eliminando archivo desde Azure Storage / Azurite. BlobPath: '{StoragePath}'...", storagePath);

        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(storagePath);
        var deleted = await blobClient.DeleteIfExistsAsync();

        if (deleted.Value)
        {
            _logger.LogInformation("[AZURE BLOB STORAGE] ✅ Archivo '{StoragePath}' eliminado exitosamente de Azure Storage / Azurite.", storagePath);
        }
        else
        {
            _logger.LogWarning("[AZURE BLOB STORAGE] ⚠️ El archivo '{StoragePath}' no existía en Azure Storage para ser eliminado.", storagePath);
        }
    }
}
