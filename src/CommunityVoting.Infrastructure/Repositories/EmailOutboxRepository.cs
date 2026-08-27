using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using CommunityVoting.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunityVoting.Infrastructure.Repositories;

public class EmailOutboxRepository : IEmailOutboxRepository
{
    private readonly CommunityVotingDbContext _context;

    public EmailOutboxRepository(CommunityVotingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailOutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _context.EmailOutboxMessages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<EmailOutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        await _context.EmailOutboxMessages.AddRangeAsync(messages, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmailOutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EmailOutboxMessages
            .Include(e => e.Meeting)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<EmailOutboxMessage>> GetPendingOrRetryableAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await _context.EmailOutboxMessages
            .Where(e => (e.Status == EmailStatus.Pending) ||
                        (e.Status == EmailStatus.Failed && e.Attempts < 5 && (e.NextAttemptAt == null || e.NextAttemptAt <= now)))
            .OrderBy(e => e.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<EmailOutboxMessage>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailOutboxMessages
            .Where(e => e.MeetingId == meetingId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailOutboxMessage message, CancellationToken cancellationToken = default)
    {
        _context.EmailOutboxMessages.Update(message);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
