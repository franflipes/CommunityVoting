using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IProposalRepository _proposalRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFileStorageService _fileStorageService;

    public DocumentService(
        IDocumentRepository documentRepository,
        IProposalRepository proposalRepository,
        IUserRepository userRepository,
        IFileStorageService fileStorageService)
    {
        _documentRepository = documentRepository;
        _proposalRepository = proposalRepository;
        _userRepository = userRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<DocumentDto?> UploadDocumentAsync(
        Guid proposalId,
        string title,
        string description,
        Stream fileStream,
        string fileName,
        string contentType,
        Guid uploadedByUserId)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId);
        if (proposal == null) return null;

        var user = await _userRepository.GetByIdAsync(uploadedByUserId);
        if (user == null) return null;

        var (storagePath, fileSize) = await _fileStorageService.SaveFileAsync(fileStream, fileName, $"proposals/{proposalId}");

        var doc = Document.Create(
            proposalId,
            title,
            description,
            fileName,
            storagePath,
            contentType,
            fileSize,
            uploadedByUserId
        );

        await _documentRepository.AddAsync(doc);

        return new DocumentDto(
            doc.Id,
            doc.ProposalId,
            doc.Title,
            doc.Description,
            doc.FileName,
            doc.ContentType,
            doc.FileSize,
            doc.UploadedByUserId,
            $"{user.Name} {user.LastName}",
            doc.UploadedAt
        );
    }

    public async Task<List<DocumentDto>> GetDocumentsByProposalAsync(Guid proposalId)
    {
        var docs = await _documentRepository.GetByProposalIdAsync(proposalId);
        return docs.Select(d => new DocumentDto(
            d.Id,
            d.ProposalId,
            d.Title,
            d.Description,
            d.FileName,
            d.ContentType,
            d.FileSize,
            d.UploadedByUserId,
            d.UploadedByUser != null ? $"{d.UploadedByUser.Name} {d.UploadedByUser.LastName}" : "Usuario",
            d.UploadedAt
        )).ToList();
    }

    public async Task<(Stream stream, string contentType, string fileName)?> DownloadDocumentAsync(Guid documentId)
    {
        var doc = await _documentRepository.GetByIdAsync(documentId);
        if (doc == null) return null;

        var (stream, contentType, fileName) = await _fileStorageService.GetFileAsync(doc.StoragePath);
        return (stream, contentType, doc.FileName);
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId)
    {
        var doc = await _documentRepository.GetByIdAsync(documentId);
        if (doc == null) return false;

        await _fileStorageService.DeleteFileAsync(doc.StoragePath);
        await _documentRepository.DeleteAsync(documentId);
        return true;
    }
}
