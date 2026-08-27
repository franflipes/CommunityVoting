using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommunityVoting.Application.Interfaces;

namespace CommunityVoting.Infrastructure.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "community-documents";

    public AzureBlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    private async Task<BlobContainerClient> GetContainerClientAsync()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        return containerClient;
    }

    public async Task<(string storagePath, long fileSize)> SaveFileAsync(Stream stream, string fileName, string folderName)
    {
        var containerClient = await GetContainerClientAsync();
        var blobName = $"{folderName}/{Guid.NewGuid()}_{Path.GetFileName(fileName)}".TrimStart('/');
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(stream, overwrite: true);
        var properties = await blobClient.GetPropertiesAsync();

        return (blobName, properties.Value.ContentLength);
    }

    public async Task<(Stream stream, string contentType, string fileName)> GetFileAsync(string storagePath)
    {
        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(storagePath);

        if (!await blobClient.ExistsAsync())
        {
            throw new FileNotFoundException("El archivo no existe en el almacenamiento Blob.", storagePath);
        }

        var downloadResult = await blobClient.DownloadStreamingAsync();
        var fileName = Path.GetFileName(storagePath);
        var contentType = downloadResult.Value.Details.ContentType ?? "application/octet-stream";

        return (downloadResult.Value.Content, contentType, fileName);
    }

    public async Task DeleteFileAsync(string storagePath)
    {
        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(storagePath);
        await blobClient.DeleteIfExistsAsync();
    }
}
