using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IEmailQueue
{
    Task EnqueueAsync(EmailQueueMessage message, CancellationToken cancellationToken = default);
}
