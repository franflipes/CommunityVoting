namespace CommunityVoting.Application.DTOs;

public record UploadDocumentRequest(
    Guid ProposalId,
    string Title,
    string Description
);

public record DocumentDto(
    Guid Id,
    Guid ProposalId,
    string Title,
    string Description,
    string FileName,
    string ContentType,
    long FileSize,
    Guid UploadedByUserId,
    string UploadedByName,
    DateTime UploadedAt
);
