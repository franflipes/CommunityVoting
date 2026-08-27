using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommunityVoting.Infrastructure.Workers;

public sealed class EmailOutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailOutboxPublisher> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(30);
    private const int BatchSize = 50;

    public EmailOutboxPublisher(
        IServiceProvider serviceProvider,
        ILogger<EmailOutboxPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailOutboxPublisher iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IEmailOutboxRepository>();
                var queue = scope.ServiceProvider.GetRequiredService<IEmailQueue>();

                var pendingMessages = await repository.GetPendingOrRetryableAsync(BatchSize, stoppingToken);

                if (pendingMessages.Any())
                {
                    _logger.LogInformation("Outbox Publisher encontró {Count} mensajes pendientes/reintentables.", pendingMessages.Count);

                    foreach (var message in pendingMessages)
                    {
                        try
                        {
                            await queue.EnqueueAsync(new EmailQueueMessage(message.Id), stoppingToken);
                            message.Status = EmailStatus.Processing;
                            message.LastAttemptAt = DateTimeOffset.UtcNow;
                            await repository.UpdateAsync(message, stoppingToken);

                            _logger.LogInformation("Mensaje Outbox {Id} encolado en Azure Storage Queue.", message.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al encolar mensaje Outbox {Id}.", message.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo de EmailOutboxPublisher.");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }
}
