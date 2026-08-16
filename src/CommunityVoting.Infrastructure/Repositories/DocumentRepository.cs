using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly CommunityVotingDbContext _context;

    public DocumentRepository(CommunityVotingDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id)
    {
        return await _context.Documents
            .Include(d => d.UploadedByUser)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Document>> GetByProposalIdAsync(Guid proposalId)
    {
        return await _context.Documents
            .Include(d => d.UploadedByUser)
            .Where(d => d.ProposalId == proposalId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Document document)
    {
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc != null)
        {
            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();
        }
    }
}
