using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Domain.Entities;

public class Proposal
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid AgendaItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }

    public MajorityType? MajorityType { get; set; }
    public decimal? MajorityPercentage { get; set; }

    public Meeting Meeting { get; set; } = null!;
    public AgendaItem AgendaItem { get; set; } = null!;
    public ICollection<ProposalOption> Options { get; set; } = new List<ProposalOption>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();

    public Proposal() { }

    public static Proposal Create(
        Guid meetingId,
        Guid agendaItemId,
        string title,
        string? description = null,
        int order = 1,
        MajorityType? majorityType = null,
        decimal? majorityPercentage = null,
        Guid? id = null)
    {
        if (meetingId == Guid.Empty) throw new ArgumentException("El ID de la reunión es obligatorio.", nameof(meetingId));
        if (agendaItemId == Guid.Empty) throw new ArgumentException("El ID del punto del orden del día es obligatorio.", nameof(agendaItemId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El título de la propuesta es obligatorio.", nameof(title));

        return new Proposal
        {
            Id = id ?? Guid.NewGuid(),
            MeetingId = meetingId,
            AgendaItemId = agendaItemId,
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Order = order,
            MajorityType = majorityType,
            MajorityPercentage = majorityPercentage
        };
    }
}
