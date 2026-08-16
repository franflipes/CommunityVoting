namespace CommunityVoting.Domain.Enums;

public enum QuorumType
{
    PercentageOfEligibleMembers = 1
}

public enum MajorityType
{
    SimpleMajority = 1,
    MajorityOfVotesCast = 2,
    QualifiedMajority = 3
}

public enum AbstentionPolicy
{
    Excluded = 1,
    IncludedInDenominator = 2,
    IncludedAsAgainst = 3
}
