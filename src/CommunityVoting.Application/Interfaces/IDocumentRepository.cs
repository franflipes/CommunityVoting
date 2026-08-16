using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id);
    Task<List<Document>> GetByProposalIdAsync(Guid proposalId);
    Task AddAsync(Document document);
    Task DeleteAsync(Guid id);
}
