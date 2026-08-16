namespace CommunityVoting.Domain.Entities;

public class AgendaItem
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }

    public Meeting Meeting { get; set; } = null!;
    public ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public AgendaItem() { }

    public static AgendaItem Create(
        Guid meetingId,
        string title,
        string? description = null,
        int order = 1,
        Guid? id = null)
    {
        if (meetingId == Guid.Empty) throw new ArgumentException("El ID de la reunión es obligatorio.", nameof(meetingId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El título del punto del día es obligatorio.", nameof(title));

        return new AgendaItem
        {
            Id = id ?? Guid.NewGuid(),
            MeetingId = meetingId,
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Order = order
        };
    }

    public void Update(string title, string? description, int order)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("El título del punto del día es obligatorio.", nameof(title));

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Order = order;
    }
}
