namespace CommunityVoting.Domain.Entities;

public class MeetingParticipant
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsPresent { get; set; } = true;

    public Meeting Meeting { get; set; } = null!;
    public User User { get; set; } = null!;

    public MeetingParticipant() { }

    public static MeetingParticipant Create(Guid meetingId, Guid userId, bool isPresent = true, Guid? id = null)
    {
        if (meetingId == Guid.Empty) throw new ArgumentException("El ID de la reunión es obligatorio.", nameof(meetingId));
        if (userId == Guid.Empty) throw new ArgumentException("El ID del usuario es obligatorio.", nameof(userId));

        return new MeetingParticipant
        {
            Id = id ?? Guid.NewGuid(),
            MeetingId = meetingId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
            IsPresent = isPresent
        };
    }
}
