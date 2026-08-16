using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Infrastructure.Repositories;

public class ProposalRepository : IProposalRepository
{
    private readonly CommunityVotingDbContext _context;

    public ProposalRepository(CommunityVotingDbContext context)
    {
        _context = context;
    }

    public async Task<Proposal?> GetByIdAsync(Guid id)
    {
        return await _context.Proposals
            .Include(p => p.Meeting)
            .Include(p => p.Options)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Proposal>> GetByMeetingIdAsync(Guid meetingId)
    {
        return await _context.Proposals
            .Include(p => p.Options)
            .Include(p => p.Documents)
            .Where(p => p.MeetingId == meetingId)
            .OrderBy(p => p.Order)
            .ToListAsync();
    }

    public async Task AddAsync(Proposal proposal)
    {
        await _context.Proposals.AddAsync(proposal);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Proposal proposal)
    {
        _context.Proposals.Update(proposal);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var proposal = await _context.Proposals.FindAsync(id);
        if (proposal != null)
        {
            _context.Proposals.Remove(proposal);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddOptionAsync(ProposalOption option)
    {
        await _context.ProposalOptions.AddAsync(option);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteOptionAsync(Guid optionId)
    {
        var option = await _context.ProposalOptions.FindAsync(optionId);
        if (option != null)
        {
            _context.ProposalOptions.Remove(option);
            await _context.SaveChangesAsync();
        }
    }
}
