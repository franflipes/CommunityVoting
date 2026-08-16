using CommunityVoting.Application.Interfaces;

namespace CommunityVoting.Infrastructure.Storage;

public class FileStorageService : IFileStorageService
{
    private readonly string _baseUploadPath;

    public FileStorageService()
    {
        _baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(_baseUploadPath))
        {
            Directory.CreateDirectory(_baseUploadPath);
        }
    }

    public async Task<(string storagePath, long fileSize)> SaveFileAsync(Stream stream, string fileName, string folderName)
    {
        var folderPath = Path.Combine(_baseUploadPath, folderName);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await stream.CopyToAsync(fileStream);
        }

        var fileInfo = new FileInfo(fullPath);
        var relativePath = Path.Combine(folderName, uniqueFileName).Replace("\\", "/");

        return (relativePath, fileInfo.Length);
    }

    public Task<(Stream stream, string contentType, string fileName)> GetFileAsync(string storagePath)
    {
        var fullPath = Path.Combine(_baseUploadPath, storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("El archivo no existe.", storagePath);
        }

        var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        var fileName = Path.GetFileName(fullPath);
        var contentType = GetContentType(fileName);

        return Task.FromResult< (Stream, string, string) >((fileStream, contentType, fileName));
    }

    public Task DeleteFileAsync(string storagePath)
    {
        var fullPath = Path.Combine(_baseUploadPath, storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".txt" => "text/plain",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}
