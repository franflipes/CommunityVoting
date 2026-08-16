using CommunityVoting.Voting.Api.Domain;
using CommunityVoting.Voting.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CommunityVoting.Voting.Api.Infrastructure;

public class VotingRuntimeManager
{
    private readonly IVotingRepository _repository;
    private readonly IHubContext<VotingHub> _hubContext;

    public VotingRuntimeManager(IVotingRepository repository, IHubContext<VotingHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    public async Task CloseSessionAsync(ProposalVotingSession session, string closedByUserName)
    {
        session.State = VotingState.Closed;
        session.ClosedAt = DateTime.UtcNow;

        int totalBallots = session.Ballots.Count;
        int totalVotesCast = session.Ballots.Count(b => b.Status == BallotStatus.Voted);
        double participationPercentage = totalBallots > 0
            ? ((double)totalVotesCast / totalBallots) * 100.0
            : 0.0;

        var votesByOption = session.Options.ToDictionary(o => o.Id.ToString(), _ => 0.0);
        foreach (var b in session.Ballots.Where(b => b.Status == BallotStatus.Voted && b.Vote != null))
        {
            var optId = b.Vote!.SelectedOptionId.ToString();
            if (!votesByOption.ContainsKey(optId)) votesByOption[optId] = 0.0;
            votesByOption[optId] += 1.0; // 1 vote per user
        }

        Guid? winningOptionId = null;
        string winningOptionLabel = "Sin votos";

        if (votesByOption.Any(kv => kv.Value > 0))
        {
            var top = votesByOption.OrderByDescending(kv => kv.Value).First();
            if (Guid.TryParse(top.Key, out var winId))
            {
                winningOptionId = winId;
                winningOptionLabel = session.Options.FirstOrDefault(o => o.Id == winId)?.Label ?? "Opción Ganadora";
            }
        }

        // Identify For / Against / Abstain votes for majority calculation
        var favorOption = session.Options.FirstOrDefault(o => o.Label.Equals("A favor", StringComparison.OrdinalIgnoreCase) || o.Label.Equals("Sí", StringComparison.OrdinalIgnoreCase));
        var againstOption = session.Options.FirstOrDefault(o => o.Label.Equals("En contra", StringComparison.OrdinalIgnoreCase) || o.Label.Equals("No", StringComparison.OrdinalIgnoreCase));

        double votesFor = favorOption != null && votesByOption.TryGetValue(favorOption.Id.ToString(), out var vf) ? vf : 0.0;
        double votesAgainst = againstOption != null && votesByOption.TryGetValue(againstOption.Id.ToString(), out var va) ? va : 0.0;
        double consideredVotes = votesFor + votesAgainst;

        // Fallback for custom options where "A favor" is not explicit
        if (favorOption == null && winningOptionId != null)
        {
            votesFor = votesByOption[winningOptionId.Value.ToString()];
            consideredVotes = totalVotesCast;
        }

        bool approved = false;
        if (totalVotesCast > 0 && (!session.RequireQuorumForVoting || session.QuorumReached))
        {
            switch (session.MajorityType)
            {
                case 1: // SimpleMajority (VotesFor > VotesAgainst)
                    approved = votesFor > votesAgainst;
                    break;
                case 2: // MajorityOfVotesCast (VotesFor / ConsideredVotes > 50%)
                    approved = consideredVotes > 0 && (votesFor / consideredVotes) > 0.50;
                    break;
                case 3: // QualifiedMajority (VotesFor / ConsideredVotes >= MajorityPercentage%)
                    double reqPct = session.MajorityPercentage ?? 66.67;
                    approved = consideredVotes > 0 && ((votesFor / consideredVotes) * 100.0) >= reqPct;
                    break;
                default:
                    approved = votesFor > votesAgainst;
                    break;
            }
        }

        var result = new VotingResult
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            OriginalProposalId = session.ProposalId,
            MeetingId = session.MeetingId,
            CommunityId = session.CommunityId,
            Title = session.Title,
            ClosedAt = session.ClosedAt.Value,
            ClosedByUserName = closedByUserName,
            EligibleMembers = session.EligibleMembers,
            PresentMembers = session.PresentMembers,
            QuorumRequired = session.QuorumRequired,
            QuorumReached = session.QuorumReached,
            TotalBallots = totalBallots,
            TotalVotesCast = totalVotesCast,
            ParticipationPercentage = participationPercentage,
            MajorityTypeUsed = session.MajorityType,
            MajorityPercentageUsed = session.MajorityPercentage,
            Approved = approved,
            WinningOptionId = winningOptionId,
            WinningOptionLabel = winningOptionLabel,
            VotesByOption = votesByOption
        };

        await _repository.AddResultAsync(result);
        await _repository.UpdateSessionAsync(session);

        // Notify SignalR clients
        await _hubContext.Clients.Groups(new[] { $"meeting-{session.MeetingId}", $"session-{session.Id}" }).SendAsync("OnSessionClosed", new
        {
            ProposalId = session.ProposalId,
            SessionId = session.Id,
            ResultId = result.Id,
            ClosedAt = session.ClosedAt,
            WinningOptionLabel = winningOptionLabel,
            Approved = approved,
            TotalVotesCast = totalVotesCast,
            ParticipationPercentage = participationPercentage
        });
    }
}
