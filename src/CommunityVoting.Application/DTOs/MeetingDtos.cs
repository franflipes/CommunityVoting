using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.DTOs;

public record MeetingDto(
    Guid Id,
    Guid CommunityId,
    string CommunityName,
    string Title,
    MeetingType Type,
    string Location,
    DateTime ScheduledAt,
    DateTime? SecondCallAt,
    DateTime VotingStart,
    DateTime VotingEnd,
    bool IsTransparent,
    VotingSettingsDto? VotingSettings,
    List<AgendaItemDto> AgendaItems,
    List<ProposalDto> Proposals
);

public record CreateMeetingRequest(
    Guid CommunityId,
    string Title,
    MeetingType Type,
    string Location,
    DateTime ScheduledAt,
    DateTime? SecondCallAt,
    bool IsTransparent = true
);

public record UpdateMeetingRequest(
    string Title,
    MeetingType Type,
    string Location,
    DateTime ScheduledAt,
    DateTime? SecondCallAt,
    bool IsTransparent = true
);

public record VotingEligibleDataDto(
    Guid CommunityId,
    Guid MeetingId,
    string MeetingTitle,
    DateTime ScheduledAt,
    int EligibleMembers,
    int PresentMembers,
    int QuorumRequired,
    bool QuorumReached,
    bool RequireQuorumForVoting,
    List<EligibleVoterDto> Voters
);

public record EligibleVoterDto(
    Guid UserId,
    string Name,
    string LastName,
    string Email,
    string? PhoneNumber,
    UserRole MemberRole
);

public record QuorumStatusDto(
    Guid MeetingId,
    int EligibleMembers,
    int PresentMembers,
    int QuorumRequired,
    decimal QuorumPercentage,
    bool QuorumReached,
    bool RequireQuorumForVoting
);

public record RecordAttendanceRequest(
    Guid UserId,
    bool IsPresent = true
);

public record MeetingParticipantDto(
    Guid Id,
    Guid MeetingId,
    Guid UserId,
    string UserName,
    string UserLastName,
    string UserEmail,
    DateTime JoinedAt,
    bool IsPresent
);
