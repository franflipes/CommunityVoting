using System.Collections.Concurrent;
using CommunityVoting.Voting.Api.Domain;

namespace CommunityVoting.Voting.Api.Infrastructure;

public interface IVotingRepository
{
    Task AddSessionAsync(ProposalVotingSession session);
    Task<ProposalVotingSession?> GetSessionAsync(Guid id);
    Task<ProposalVotingSession?> GetSessionByProposalIdAsync(Guid proposalId);
    Task UpdateSessionAsync(ProposalVotingSession session);
    Task RemoveSessionAsync(Guid id);

    Task AddResultAsync(VotingResult result);
    Task<VotingResult?> GetResultAsync(Guid id);
    Task<VotingResult?> GetResultByProposalIdAsync(Guid proposalId);
    Task<List<VotingResult>> GetResultsByMeetingIdAsync(Guid meetingId);

    Task<List<ProposalVotingSession>> GetActiveSessionsAsync();
}

public class InMemoryVotingRepository : IVotingRepository
{
    private readonly ConcurrentDictionary<Guid, ProposalVotingSession> _sessions = new();
    private readonly ConcurrentDictionary<Guid, VotingResult> _results = new();

    public Task AddSessionAsync(ProposalVotingSession session)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task<ProposalVotingSession?> GetSessionAsync(Guid id)
    {
        _sessions.TryGetValue(id, out var session);
        return Task.FromResult(session);
    }

    public Task<ProposalVotingSession?> GetSessionByProposalIdAsync(Guid proposalId)
    {
        var session = _sessions.Values.FirstOrDefault(s => s.ProposalId == proposalId);
        return Task.FromResult(session);
    }

    public Task UpdateSessionAsync(ProposalVotingSession session)
    {
        _sessions[session.Id] = session;
        return Task.CompletedTask;
    }

    public Task RemoveSessionAsync(Guid id)
    {
        _sessions.TryRemove(id, out _);
        return Task.CompletedTask;
    }

    public Task AddResultAsync(VotingResult result)
    {
        _results[result.Id] = result;
        return Task.CompletedTask;
    }

    public Task<VotingResult?> GetResultAsync(Guid id)
    {
        _results.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<VotingResult?> GetResultByProposalIdAsync(Guid proposalId)
    {
        var result = _results.Values.FirstOrDefault(r => r.OriginalProposalId == proposalId);
        return Task.FromResult(result);
    }

    public Task<List<VotingResult>> GetResultsByMeetingIdAsync(Guid meetingId)
    {
        var list = _results.Values.Where(r => r.MeetingId == meetingId).ToList();
        return Task.FromResult(list);
    }

    public Task<List<ProposalVotingSession>> GetActiveSessionsAsync()
    {
        var list = _sessions.Values.ToList();
        return Task.FromResult(list);
    }
}
