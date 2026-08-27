using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.DTOs;

public record ProposalDto(
    Guid Id,
    Guid MeetingId,
    Guid AgendaItemId,
    string Title,
    string Description,
    int Order,
    MajorityType? MajorityType,
    decimal? MajorityPercentage,
    List<ProposalOptionDto> Options,
    List<DocumentDto> Documents
);

public record ProposalOptionDto(
    Guid Id,
    Guid ProposalId,
    string Label
);

public record CreateProposalRequest(
    Guid MeetingId,
    Guid AgendaItemId,
    string Title,
    string? Description,
    int Order = 1,
    MajorityType? MajorityType = null,
    decimal? MajorityPercentage = null,
    List<string>? Options = null
);

public record UpdateProposalMajorityRequest(
    MajorityType? MajorityType,
    decimal? MajorityPercentage
);

public record UpdateProposalRequest(
    string Title,
    string? Description,
    int Order = 1,
    MajorityType? MajorityType = null,
    decimal? MajorityPercentage = null,
    List<string>? Options = null
);

public record CreateProposalOptionRequest(
    Guid ProposalId,
    string Label
);
