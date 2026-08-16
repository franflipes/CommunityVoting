using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Services;

public class AgendaItemService
{
    private readonly IAgendaItemRepository _agendaItemRepository;
    private readonly IMeetingRepository _meetingRepository;

    public AgendaItemService(IAgendaItemRepository agendaItemRepository, IMeetingRepository meetingRepository)
    {
        _agendaItemRepository = agendaItemRepository;
        _meetingRepository = meetingRepository;
    }

    public async Task<AgendaItemDto?> CreateAsync(CreateAgendaItemRequest request)
    {
        var meeting = await _meetingRepository.GetByIdAsync(request.MeetingId);
        if (meeting == null) return null;

        var existingItems = (await _agendaItemRepository.GetByMeetingIdAsync(request.MeetingId)).ToList();
        int nextOrder = existingItems.Count > 0 ? existingItems.Max(i => i.Order) + 1 : 1;
        int finalOrder = (request.Order <= 0 || (request.Order == 1 && existingItems.Count > 0))
            ? nextOrder
            : request.Order;

        var agendaItem = AgendaItem.Create(request.MeetingId, request.Title, request.Description, finalOrder);
        var created = await _agendaItemRepository.AddAsync(agendaItem);

        return MapToDto(created);
    }

    public async Task<IEnumerable<AgendaItemDto>> GetByMeetingIdAsync(Guid meetingId)
    {
        var items = await _agendaItemRepository.GetByMeetingIdAsync(meetingId);
        return items.Select(MapToDto);
    }

    public async Task<AgendaItemDto?> GetByIdAsync(Guid id)
    {
        var item = await _agendaItemRepository.GetByIdAsync(id);
        return item == null ? null : MapToDto(item);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await _agendaItemRepository.GetByIdAsync(id);
        if (item == null) return false;

        await _agendaItemRepository.DeleteAsync(id);
        return true;
    }

    public static AgendaItemDto MapToDto(AgendaItem item)
    {
        return new AgendaItemDto(
            item.Id,
            item.MeetingId,
            item.Title,
            item.Description,
            item.Order,
            item.Proposals?.Select(ProposalService.MapToDto).ToList() ?? new List<ProposalDto>()
        );
    }
}
