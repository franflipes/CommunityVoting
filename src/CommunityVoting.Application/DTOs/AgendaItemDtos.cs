namespace CommunityVoting.Application.DTOs;

public record AgendaItemDto(
    Guid Id,
    Guid MeetingId,
    string Title,
    string Description,
    int Order,
    List<ProposalDto> Proposals
);

public record CreateAgendaItemRequest(
    Guid MeetingId,
    string Title,
    string? Description,
    int Order = 1
);

public record UpdateAgendaItemRequest(
    string Title,
    string? Description,
    int Order
);
