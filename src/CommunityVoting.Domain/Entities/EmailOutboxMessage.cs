using System;
using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Domain.Entities;

public class EmailOutboxMessage
{
    public Guid Id { get; set; }
    public Guid? MeetingId { get; set; }
    public Guid? UserId { get; set; }
    public string Type { get; set; } = null!;
    public string Recipient { get; set; } = null!;
    public string Template { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public int Attempts { get; set; } = 0;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public string? Error { get; set; }

    public Meeting? Meeting { get; set; }
    public User? User { get; set; }

    public EmailOutboxMessage() { }

    public static EmailOutboxMessage Create(
        string type,
        string recipient,
        string template,
        string payload,
        Guid? meetingId = null,
        Guid? userId = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("El tipo de mensaje es obligatorio.", nameof(type));
        if (string.IsNullOrWhiteSpace(recipient)) throw new ArgumentException("El destinatario es obligatorio.", nameof(recipient));
        if (string.IsNullOrWhiteSpace(template)) throw new ArgumentException("La plantilla es obligatoria.", nameof(template));
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("El payload es obligatorio.", nameof(payload));

        return new EmailOutboxMessage
        {
            Id = id ?? Guid.NewGuid(),
            MeetingId = meetingId,
            UserId = userId,
            Type = type.Trim(),
            Recipient = recipient.Trim().ToLowerInvariant(),
            Template = template.Trim(),
            Payload = payload,
            Status = EmailStatus.Pending,
            Attempts = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
