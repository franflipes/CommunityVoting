using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Infrastructure.Repositories;

public class AgendaItemRepository : IAgendaItemRepository
{
    private readonly CommunityVotingDbContext _context;

    public AgendaItemRepository(CommunityVotingDbContext context)
    {
        _context = context;
    }

    public async Task<AgendaItem?> GetByIdAsync(Guid id)
    {
        return await _context.AgendaItems
            .Include(ai => ai.Proposals)
                .ThenInclude(p => p.Options)
            .Include(ai => ai.Proposals)
                .ThenInclude(p => p.Documents)
            .FirstOrDefaultAsync(ai => ai.Id == id);
    }

    public async Task<IEnumerable<AgendaItem>> GetByMeetingIdAsync(Guid meetingId)
    {
        return await _context.AgendaItems
            .Include(ai => ai.Proposals)
                .ThenInclude(p => p.Options)
            .Include(ai => ai.Proposals)
                .ThenInclude(p => p.Documents)
            .Where(ai => ai.MeetingId == meetingId)
            .OrderBy(ai => ai.Order)
            .ToListAsync();
    }

    public async Task<AgendaItem> AddAsync(AgendaItem item)
    {
        await _context.AgendaItems.AddAsync(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task UpdateAsync(AgendaItem item)
    {
        _context.AgendaItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await _context.AgendaItems.FindAsync(id);
        if (item != null)
        {
            _context.AgendaItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}
