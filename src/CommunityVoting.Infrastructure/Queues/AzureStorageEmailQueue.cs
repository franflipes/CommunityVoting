using System.Text.Json;
using Azure.Storage.Queues;
using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CommunityVoting.Infrastructure.Queues;

public class AzureStorageEmailQueue : IEmailQueue
{
    private readonly QueueClient _queueClient;

    public AzureStorageEmailQueue(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("queues")
            ?? configuration.GetConnectionString("storage")
            ?? configuration.GetConnectionString("AzureStorage")
            ?? "UseDevelopmentStorage=true";

        var queueName = configuration["Queue:EmailQueueName"] ?? "email-outbox";

        _queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions
        {
            MessageEncoding = QueueMessageEncoding.Base64
        });
    }

    public async Task EnqueueAsync(EmailQueueMessage message, CancellationToken cancellationToken = default)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var body = JsonSerializer.Serialize(message);
        await _queueClient.SendMessageAsync(body, cancellationToken);
    }
}
