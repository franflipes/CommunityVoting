using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Domain.Entities;

public class VotingSettings
{
    public Guid Id { get; set; }
    public bool QuorumEnabled { get; set; } = true;
    public QuorumType QuorumType { get; set; } = QuorumType.PercentageOfEligibleMembers;
    public decimal QuorumPercentage { get; set; } = 50.0m;
    public bool RequireQuorumForVoting { get; set; } = true;

    public MajorityType DefaultMajorityType { get; set; } = MajorityType.SimpleMajority;
    public decimal? DefaultMajorityPercentage { get; set; }
    public AbstentionPolicy AbstentionPolicy { get; set; } = AbstentionPolicy.Excluded;

    public VotingSettings() { }

    public static VotingSettings CreateDefault(Guid? id = null)
    {
        return new VotingSettings
        {
            Id = id ?? Guid.NewGuid(),
            QuorumEnabled = true,
            QuorumType = QuorumType.PercentageOfEligibleMembers,
            QuorumPercentage = 50.0m,
            RequireQuorumForVoting = true,
            DefaultMajorityType = MajorityType.SimpleMajority,
            DefaultMajorityPercentage = null,
            AbstentionPolicy = AbstentionPolicy.Excluded
        };
    }

    public VotingSettings Clone()
    {
        return new VotingSettings
        {
            Id = Guid.NewGuid(),
            QuorumEnabled = QuorumEnabled,
            QuorumType = QuorumType,
            QuorumPercentage = QuorumPercentage,
            RequireQuorumForVoting = RequireQuorumForVoting,
            DefaultMajorityType = DefaultMajorityType,
            DefaultMajorityPercentage = DefaultMajorityPercentage,
            AbstentionPolicy = AbstentionPolicy
        };
    }
}
