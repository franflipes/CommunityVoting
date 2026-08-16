using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Domain.Entities;

public class Meeting
{
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public MeetingType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public DateTime? SecondCallAt { get; set; }
    public DateTime VotingStart { get; set; }
    public DateTime VotingEnd { get; set; }
    public bool IsTransparent { get; set; } = true;
    public Guid VotingSettingsId { get; set; }

    public Community Community { get; set; } = null!;
    public VotingSettings VotingSettings { get; set; } = null!;
    public ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    public ICollection<AgendaItem> AgendaItems { get; set; } = new List<AgendaItem>();
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public Meeting() { }

    public static Meeting Create(
        Guid communityId,
        string title,
        MeetingType type,
        string location,
        DateTime scheduledAt,
        DateTime? secondCallAt = null,
        bool isTransparent = true,
        VotingSettings? communitySettings = null,
        Guid? id = null)
    {
        if (communityId == Guid.Empty) throw new ArgumentException("El ID de la comunidad es obligatorio.", nameof(communityId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El título es obligatorio.", nameof(title));
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("La ubicación es obligatoria.", nameof(location));

        var meetingId = id ?? Guid.NewGuid();
        var meetingSettings = communitySettings != null ? communitySettings.Clone() : VotingSettings.CreateDefault();

        return new Meeting
        {
            Id = meetingId,
            CommunityId = communityId,
            Title = title.Trim(),
            Type = type,
            Location = location.Trim(),
            ScheduledAt = scheduledAt,
            SecondCallAt = secondCallAt,
            IsTransparent = isTransparent,
            VotingStart = scheduledAt.AddDays(-7),
            VotingEnd = scheduledAt,
            VotingSettingsId = meetingSettings.Id,
            VotingSettings = meetingSettings
        };
    }

    public void Update(
        string title,
        MeetingType type,
        string location,
        DateTime scheduledAt,
        DateTime? secondCallAt,
        bool isTransparent)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El título es obligatorio.", nameof(title));
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("La ubicación es obligatoria.", nameof(location));

        Title = title.Trim();
        Type = type;
        Location = location.Trim();
        ScheduledAt = scheduledAt;
        SecondCallAt = secondCallAt;
        IsTransparent = isTransparent;
        VotingEnd = scheduledAt;
    }
}
