using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Interfaces;

public interface IEmailOutboxRepository
{
    Task AddAsync(EmailOutboxMessage message, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<EmailOutboxMessage> messages, CancellationToken cancellationToken = default);
    Task<EmailOutboxMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<EmailOutboxMessage>> GetPendingOrRetryableAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<List<EmailOutboxMessage>> GetByMeetingIdAsync(Guid meetingId, CancellationToken cancellationToken = default);
    Task UpdateAsync(EmailOutboxMessage message, CancellationToken cancellationToken = default);
}
