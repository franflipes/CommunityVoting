using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Interfaces;

public interface IJwtProvider
{
    string Generate(User user, string? authMethod = null);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface IFileStorageService
{
    Task<(string storagePath, long fileSize)> SaveFileAsync(Stream stream, string fileName, string folderName);
    Task<(Stream stream, string contentType, string fileName)> GetFileAsync(string storagePath);
    Task DeleteFileAsync(string storagePath);
}
