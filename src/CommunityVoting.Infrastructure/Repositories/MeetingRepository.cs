using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Infrastructure.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly CommunityVotingDbContext _context;

    public MeetingRepository(CommunityVotingDbContext context)
    {
        _context = context;
    }

    public async Task<Meeting?> GetByIdAsync(Guid id)
    {
        return await _context.Meetings
            .Include(m => m.Community)
            .Include(m => m.VotingSettings)
            .Include(m => m.Participants)
                .ThenInclude(p => p.User)
            .Include(m => m.AgendaItems.OrderBy(ai => ai.Order))
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Meeting?> GetByIdWithProposalsAsync(Guid id)
    {
        return await _context.Meetings
            .Include(m => m.Community)
            .Include(m => m.VotingSettings)
            .Include(m => m.Participants)
                .ThenInclude(p => p.User)
            .Include(m => m.AgendaItems.OrderBy(ai => ai.Order))
                .ThenInclude(ai => ai.Proposals.OrderBy(p => p.Order))
                    .ThenInclude(p => p.Options)
            .Include(m => m.AgendaItems)
                .ThenInclude(ai => ai.Proposals)
                    .ThenInclude(p => p.Documents)
            .Include(m => m.Proposals.OrderBy(p => p.Order))
                .ThenInclude(p => p.Options)
            .Include(m => m.Proposals)
                .ThenInclude(p => p.Documents)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Meeting>> GetAllAsync()
    {
        return await _context.Meetings
            .Include(m => m.Community)
            .Include(m => m.VotingSettings)
            .OrderByDescending(m => m.ScheduledAt)
            .ToListAsync();
    }

    public async Task<List<Meeting>> GetByCommunityIdAsync(Guid communityId)
    {
        return await _context.Meetings
            .Include(m => m.VotingSettings)
            .Where(m => m.CommunityId == communityId)
            .OrderByDescending(m => m.ScheduledAt)
            .ToListAsync();
    }

    public async Task AddAsync(Meeting meeting)
    {
        await _context.Meetings.AddAsync(meeting);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Meeting meeting)
    {
        _context.Meetings.Update(meeting);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var meeting = await _context.Meetings.FindAsync(id);
        if (meeting != null)
        {
            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddParticipantAsync(MeetingParticipant participant)
    {
        var existing = await _context.MeetingParticipants
            .FirstOrDefaultAsync(p => p.MeetingId == participant.MeetingId && p.UserId == participant.UserId);

        if (existing == null)
        {
            await _context.MeetingParticipants.AddAsync(participant);
        }
        else
        {
            existing.IsPresent = participant.IsPresent;
            _context.MeetingParticipants.Update(existing);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<MeetingParticipant?> GetParticipantAsync(Guid meetingId, Guid userId)
    {
        return await _context.MeetingParticipants
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.MeetingId == meetingId && p.UserId == userId);
    }

    public async Task<List<MeetingParticipant>> GetParticipantsAsync(Guid meetingId)
    {
        return await _context.MeetingParticipants
            .Include(p => p.User)
            .Where(p => p.MeetingId == meetingId)
            .ToListAsync();
    }

    public async Task UpdateVotingSettingsAsync(VotingSettings settings)
    {
        _context.VotingSettings.Update(settings);
        await _context.SaveChangesAsync();
    }
}
