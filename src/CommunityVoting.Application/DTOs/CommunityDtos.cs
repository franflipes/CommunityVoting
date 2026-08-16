using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.DTOs;

public record CreateCommunityRequest(
    string Name,
    string Address,
    string? Cif
);

public record CommunityDto(
    Guid Id,
    string Name,
    string Address,
    string? Cif,
    Guid CreatedByUserId,
    string CreatedByName,
    int MembersCount,
    int MeetingsCount
);

public record AddCommunityMemberRequest(
    string Name,
    string Email,
    string? LastName = null,
    string? PhoneNumber = null,
    UserRole MemberRole = UserRole.CommunityMember
);

public record CommunityMemberDto(
    Guid Id,
    Guid CommunityId,
    Guid UserId,
    string UserName,
    string UserLastName,
    string UserEmail,
    UserRole MemberRole,
    bool HasVotingRights,
    bool IsActive,
    DateTime JoinedAt
);

public record VotingSettingsDto(
    Guid Id,
    bool QuorumEnabled,
    QuorumType QuorumType,
    decimal QuorumPercentage,
    bool RequireQuorumForVoting,
    MajorityType DefaultMajorityType,
    decimal? DefaultMajorityPercentage,
    AbstentionPolicy AbstentionPolicy
);

public record UpdateVotingSettingsRequest(
    bool QuorumEnabled,
    QuorumType QuorumType,
    decimal QuorumPercentage,
    bool RequireQuorumForVoting,
    MajorityType DefaultMajorityType,
    decimal? DefaultMajorityPercentage,
    AbstentionPolicy AbstentionPolicy
);

public record UpdateMemberVotingRightsRequest(
    bool HasVotingRights,
    bool IsActive
);
