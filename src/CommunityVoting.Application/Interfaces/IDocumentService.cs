using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentDto?> UploadDocumentAsync(
        Guid proposalId,
        string title,
        string description,
        Stream fileStream,
        string fileName,
        string contentType,
        Guid uploadedByUserId);

    Task<List<DocumentDto>> GetDocumentsByProposalAsync(Guid proposalId);
    Task<(Stream stream, string contentType, string fileName)?> DownloadDocumentAsync(Guid documentId);
    Task<bool> DeleteDocumentAsync(Guid documentId);
}
