using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IAgendaItemService
{
    Task<AgendaItemDto?> CreateAsync(CreateAgendaItemRequest request);
    Task<IEnumerable<AgendaItemDto>> GetByMeetingIdAsync(Guid meetingId);
    Task<AgendaItemDto?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
