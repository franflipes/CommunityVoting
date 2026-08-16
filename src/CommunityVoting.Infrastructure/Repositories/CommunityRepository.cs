using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Infrastructure.Repositories;

public class CommunityRepository : ICommunityRepository
{
    private readonly CommunityVotingDbContext _context;

    public CommunityRepository(CommunityVotingDbContext context)
    {
        _context = context;
    }

    public async Task<Community?> GetByIdAsync(Guid id)
    {
        return await _context.Communities
            .Include(c => c.CreatedBy)
            .Include(c => c.VotingSettings)
            .Include(c => c.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Community>> GetAllAsync()
    {
        return await _context.Communities
            .Include(c => c.CreatedBy)
            .Include(c => c.VotingSettings)
            .ToListAsync();
    }

    public async Task<List<Community>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Communities
            .Include(c => c.CreatedBy)
            .Include(c => c.VotingSettings)
            .Where(c => c.CreatedByUserId == userId || c.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task AddAsync(Community community)
    {
        await _context.Communities.AddAsync(community);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Community community)
    {
        _context.Communities.Update(community);
        await _context.SaveChangesAsync();
    }

    public async Task AddMemberAsync(CommunityMember member)
    {
        var existing = await _context.CommunityMembers
            .FirstOrDefaultAsync(m => m.CommunityId == member.CommunityId && m.UserId == member.UserId);

        if (existing == null)
        {
            await _context.CommunityMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateMemberAsync(CommunityMember member)
    {
        _context.CommunityMembers.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateVotingSettingsAsync(VotingSettings settings)
    {
        _context.VotingSettings.Update(settings);
        await _context.SaveChangesAsync();
    }

    public async Task<CommunityMember?> GetMemberAsync(Guid communityId, Guid userId)
    {
        return await _context.CommunityMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.CommunityId == communityId && m.UserId == userId);
    }

    public async Task<List<CommunityMember>> GetMembersAsync(Guid communityId)
    {
        return await _context.CommunityMembers
            .Include(m => m.User)
            .Where(m => m.CommunityId == communityId)
            .ToListAsync();
    }
}
