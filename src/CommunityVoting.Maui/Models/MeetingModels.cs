namespace CommunityVoting.Maui.Models;

public class MeetingItem
{
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public string CommunityName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Type { get; set; }
    public string TypeDisplay => Type == 1 ? "Extraordinaria" : "Ordinaria";
    public string Location { get; set; } = string.Empty;
    public DateTime ScheduledAt { get; set; }
    public DateTime? SecondCallAt { get; set; }
    public bool IsTransparent { get; set; }
    public List<AgendaItemDto> AgendaItems { get; set; } = new();
    public List<ProposalDto> Proposals { get; set; } = new();
}

public class AgendaItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class ProposalDto
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Guid AgendaItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public List<ProposalOptionDto> Options { get; set; } = new();
}

public class ProposalOptionDto
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Text => Label;
}
