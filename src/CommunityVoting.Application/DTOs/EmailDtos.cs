using System;
using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.DTOs;

public record EmailQueueMessage(Guid OutboxMessageId);

public sealed class MeetingInvitationEmailModel
{
    public string FirstName { get; init; } = null!;
    public string CommunityName { get; init; } = null!;
    public string MeetingTitle { get; init; } = null!;
    public string MeetingDate { get; init; } = null!;
    public string MeetingTime { get; init; } = null!;
    public string AccessUrl { get; init; } = null!;
    public string AccessCode { get; init; } = null!;
}

public record VoterInvitationStatusDto(
    Guid OutboxId,
    Guid MeetingId,
    Guid UserId,
    string UserName,
    string UserEmail,
    EmailStatus Status,
    int Attempts,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    DateTimeOffset? LastAttemptAt,
    string? Error
);
