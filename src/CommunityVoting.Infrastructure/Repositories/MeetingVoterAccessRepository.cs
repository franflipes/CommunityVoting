using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Infrastructure.Repositories;

public class MeetingVoterAccessRepository : IMeetingVoterAccessRepository
{
    private readonly CommunityVotingDbContext _dbContext;

    public MeetingVoterAccessRepository(CommunityVotingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MeetingVoterAccess?> GetByMeetingAndUserAsync(Guid meetingId, Guid userId)
    {
        return await _dbContext.MeetingVoterAccesses
            .Include(mva => mva.Meeting)
            .Include(mva => mva.User)
            .FirstOrDefaultAsync(mva => mva.MeetingId == meetingId && mva.UserId == userId);
    }

    public async Task<MeetingVoterAccess?> GetByTokenHashAsync(string tokenHash)
    {
        return await _dbContext.MeetingVoterAccesses
            .Include(mva => mva.Meeting)
            .Include(mva => mva.User)
            .FirstOrDefaultAsync(mva => mva.TokenHash == tokenHash);
    }

    public async Task<List<MeetingVoterAccess>> GetByMeetingIdAsync(Guid meetingId)
    {
        return await _dbContext.MeetingVoterAccesses
            .Include(mva => mva.User)
            .Where(mva => mva.MeetingId == meetingId)
            .ToListAsync();
    }

    public async Task AddAsync(MeetingVoterAccess access)
    {
        await _dbContext.MeetingVoterAccesses.AddAsync(access);
        await _dbContext.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<MeetingVoterAccess> accesses)
    {
        await _dbContext.MeetingVoterAccesses.AddRangeAsync(accesses);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(MeetingVoterAccess access)
    {
        _dbContext.MeetingVoterAccesses.Update(access);
        await _dbContext.SaveChangesAsync();
    }
}
