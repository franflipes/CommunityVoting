using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Infrastructure.Repositories;

public class CommunityInvitationRepository : ICommunityInvitationRepository
{
    private readonly CommunityVotingDbContext _context;

    public CommunityInvitationRepository(CommunityVotingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CommunityInvitation invitation)
    {
        await _context.CommunityInvitations.AddAsync(invitation);
        await _context.SaveChangesAsync();
    }

    public async Task<CommunityInvitation?> GetByTokenAsync(string token)
    {
        return await _context.CommunityInvitations
            .Include(ci => ci.Community)
            .Include(ci => ci.CreatedByUser)
            .FirstOrDefaultAsync(ci => ci.Token == token);
    }

    public async Task<List<CommunityInvitation>> GetByCommunityIdAsync(Guid communityId)
    {
        return await _context.CommunityInvitations
            .Include(ci => ci.CreatedByUser)
            .Where(ci => ci.CommunityId == communityId)
            .OrderByDescending(ci => ci.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateAsync(CommunityInvitation invitation)
    {
        _context.CommunityInvitations.Update(invitation);
        await _context.SaveChangesAsync();
    }
}
