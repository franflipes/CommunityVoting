using CommunityVoting.Voting.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Voting.Api.Infrastructure;

public class EfVotingRepository : IVotingRepository
{
    private readonly VotingDbContext _db;

    public EfVotingRepository(VotingDbContext db)
    {
        _db = db;
    }

    public async Task AddSessionAsync(ProposalVotingSession session)
    {
        await _db.Sessions.AddAsync(session);
        await _db.SaveChangesAsync();
    }

    public async Task<ProposalVotingSession?> GetSessionAsync(Guid id)
    {
        return await _db.Sessions.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ProposalVotingSession?> GetSessionByProposalIdAsync(Guid proposalId)
    {
        return await _db.Sessions.FirstOrDefaultAsync(s => s.ProposalId == proposalId);
    }

    public async Task UpdateSessionAsync(ProposalVotingSession session)
    {
        _db.Sessions.Update(session);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveSessionAsync(Guid id)
    {
        var session = await _db.Sessions.FindAsync(id);
        if (session != null)
        {
            _db.Sessions.Remove(session);
            await _db.SaveChangesAsync();
        }
    }

    public async Task AddResultAsync(VotingResult result)
    {
        await _db.Results.AddAsync(result);
        await _db.SaveChangesAsync();
    }

    public async Task<VotingResult?> GetResultAsync(Guid id)
    {
        return await _db.Results.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<VotingResult?> GetResultByProposalIdAsync(Guid proposalId)
    {
        return await _db.Results.FirstOrDefaultAsync(r => r.OriginalProposalId == proposalId);
    }

    public async Task<List<VotingResult>> GetResultsByMeetingIdAsync(Guid meetingId)
    {
        return await _db.Results.Where(r => r.MeetingId == meetingId).ToListAsync();
    }

    public async Task<List<ProposalVotingSession>> GetActiveSessionsAsync()
    {
        return await _db.Sessions.ToListAsync();
    }
}
