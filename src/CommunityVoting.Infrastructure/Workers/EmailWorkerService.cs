using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommunityVoting.Infrastructure.Workers;

public sealed class EmailWorkerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly QueueClient _queueClient;
    private readonly ILogger<EmailWorkerService> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(3);

    public EmailWorkerService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<EmailWorkerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailWorkerService iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _queueClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);
                QueueMessage[] messages = await _queueClient.ReceiveMessagesAsync(maxMessages: 10, visibilityTimeout: TimeSpan.FromSeconds(30), cancellationToken: stoppingToken);

                if (messages != null && messages.Length > 0)
                {
                    _logger.LogInformation("EmailWorker recibió {Count} mensajes de la cola.", messages.Length);

                    foreach (var queueMessage in messages)
                    {
                        await ProcessQueueMessageAsync(queueMessage, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el ciclo principal de EmailWorkerService.");
            }

            await Task.Delay(_pollInterval, stoppingToken);
        }
    }

    private async Task ProcessQueueMessageAsync(QueueMessage queueMessage, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmailOutboxRepository>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        EmailQueueMessage? payloadMsg = null;
        try
        {
            payloadMsg = JsonSerializer.Deserialize<EmailQueueMessage>(queueMessage.Body.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mensaje de cola con formato JSON no válido. Eliminando de la cola.");
            await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);
            return;
        }

        if (payloadMsg == null || payloadMsg.OutboxMessageId == Guid.Empty)
        {
            await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);
            return;
        }

        var outboxMessage = await repository.GetByIdAsync(payloadMsg.OutboxMessageId, cancellationToken);

        if (outboxMessage == null)
        {
            _logger.LogWarning("Mensaje Outbox {OutboxId} no encontrado en base de datos. Eliminando de la cola.", payloadMsg.OutboxMessageId);
            await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);
            return;
        }

        // Idempotency check: If already sent, skip
        if (outboxMessage.Status == EmailStatus.Sent)
        {
            _logger.LogInformation("El mensaje Outbox {OutboxId} ya fue enviado previamente. Omitiendo.", outboxMessage.Id);
            await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);
            return;
        }

        try
        {
            var emailModel = JsonSerializer.Deserialize<MeetingInvitationEmailModel>(outboxMessage.Payload);
            if (emailModel == null)
            {
                throw new InvalidOperationException("Payload del mensaje Outbox no pudo ser deserializado.");
            }

            await emailService.SendMeetingInvitationAsync(outboxMessage.Recipient, emailModel, cancellationToken);

            outboxMessage.Status = EmailStatus.Sent;
            outboxMessage.ProcessedAt = DateTimeOffset.UtcNow;
            outboxMessage.LastAttemptAt = DateTimeOffset.UtcNow;
            outboxMessage.Error = null;

            await repository.UpdateAsync(outboxMessage, cancellationToken);
            await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);

            _logger.LogInformation("Mensaje Outbox {OutboxId} enviado exitosamente a {Recipient}.", outboxMessage.Id, outboxMessage.Recipient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo al enviar correo para mensaje Outbox {OutboxId}.", outboxMessage.Id);

            outboxMessage.Attempts++;
            outboxMessage.LastAttemptAt = DateTimeOffset.UtcNow;
            outboxMessage.Error = ex.Message;
            outboxMessage.Status = EmailStatus.Failed;

            // Calculate backoff
            double minutesDelay = Math.Min(60, Math.Pow(2, outboxMessage.Attempts));
            outboxMessage.NextAttemptAt = DateTimeOffset.UtcNow.AddMinutes(minutesDelay);

            await repository.UpdateAsync(outboxMessage, cancellationToken);
            await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt, cancellationToken);
        }
    }
}
