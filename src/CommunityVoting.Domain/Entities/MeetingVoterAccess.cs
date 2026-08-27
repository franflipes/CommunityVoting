using System;

namespace CommunityVoting.Domain.Entities;

public class MeetingVoterAccess
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public int FailedAttempts { get; set; } = 0;

    public Meeting Meeting { get; set; } = null!;
    public User User { get; set; } = null!;

    public MeetingVoterAccess() { }

    public static MeetingVoterAccess Create(
        Guid meetingId,
        Guid userId,
        string tokenHash,
        string codeHash,
        DateTimeOffset? expiresAt = null,
        Guid? id = null)
    {
        if (meetingId == Guid.Empty) throw new ArgumentException("El ID de la reunión es obligatorio.", nameof(meetingId));
        if (userId == Guid.Empty) throw new ArgumentException("El ID del usuario es obligatorio.", nameof(userId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("El hash del token es obligatorio.", nameof(tokenHash));
        if (string.IsNullOrWhiteSpace(codeHash)) throw new ArgumentException("El hash del código es obligatorio.", nameof(codeHash));

        return new MeetingVoterAccess
        {
            Id = id ?? Guid.NewGuid(),
            MeetingId = meetingId,
            UserId = userId,
            TokenHash = tokenHash,
            CodeHash = codeHash,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            FailedAttempts = 0
        };
    }
}
