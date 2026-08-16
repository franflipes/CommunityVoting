namespace CommunityVoting.Voting.Api.Domain;

public enum VotingState
{
    Created = 0,
    Prepared = 1,
    Open = 2,
    Closed = 3,
    Expired = 4
}

public enum BallotStatus
{
    Pending = 0,
    Voted = 1
}

public class VoteOption
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class Vote
{
    public Guid SelectedOptionId { get; set; }
    public string SelectedOptionLabel { get; set; } = string.Empty;
    public DateTime CastAt { get; set; }
}

public class Ballot
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public BallotStatus Status { get; set; } = BallotStatus.Pending;
    public Vote? Vote { get; set; }
}

public class ProposalVotingSession
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public Guid MeetingId { get; set; }
    public Guid CommunityId { get; set; }
    public Guid? AgendaItemId { get; set; }
    public string AgendaItemTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 1;
    public string MeetingName { get; set; } = string.Empty;

    public VotingState State { get; set; } = VotingState.Created;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? ExpirationTime { get; set; }

    public int EligibleMembers { get; set; }
    public int PresentMembers { get; set; }
    public int QuorumRequired { get; set; }
    public bool QuorumReached { get; set; }
    public bool RequireQuorumForVoting { get; set; } = true;

    public int MajorityType { get; set; } = 1; // 1: SimpleMajority, 2: MajorityOfVotesCast, 3: QualifiedMajority
    public double? MajorityPercentage { get; set; }

    public List<VoteOption> Options { get; set; } = new();
    public List<Ballot> Ballots { get; set; } = new();
}

public class VotingResult
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid OriginalProposalId { get; set; }
    public Guid MeetingId { get; set; }
    public Guid CommunityId { get; set; }
    public Guid? AgendaItemId { get; set; }
    public string AgendaItemTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime ClosedAt { get; set; }
    public string ClosedByUserName { get; set; } = string.Empty;

    public int EligibleMembers { get; set; }
    public int PresentMembers { get; set; }
    public int QuorumRequired { get; set; }
    public bool QuorumReached { get; set; }

    public int TotalBallots { get; set; }
    public int TotalVotesCast { get; set; }
    public double ParticipationPercentage { get; set; }

    public int MajorityTypeUsed { get; set; } = 1;
    public double? MajorityPercentageUsed { get; set; }
    public bool Approved { get; set; }

    public Guid? WinningOptionId { get; set; }
    public string WinningOptionLabel { get; set; } = string.Empty;

    public Dictionary<string, double> VotesByOption { get; set; } = new();
}
