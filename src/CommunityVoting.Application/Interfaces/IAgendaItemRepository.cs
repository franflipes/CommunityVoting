using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Interfaces;

public interface IAgendaItemRepository
{
    Task<AgendaItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<AgendaItem>> GetByMeetingIdAsync(Guid meetingId);
    Task<AgendaItem> AddAsync(AgendaItem item);
    Task UpdateAsync(AgendaItem item);
    Task DeleteAsync(Guid id);
}
