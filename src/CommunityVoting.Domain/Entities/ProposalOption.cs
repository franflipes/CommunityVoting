namespace CommunityVoting.Domain.Entities;

public class ProposalOption
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public string Label { get; set; } = string.Empty;

    public Proposal Proposal { get; set; } = null!;

    public ProposalOption() { }

    public static ProposalOption Create(Guid proposalId, string label, Guid? id = null)
    {
        if (proposalId == Guid.Empty) throw new ArgumentException("El ID de la propuesta es obligatorio.", nameof(proposalId));
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("La etiqueta de la opción es obligatoria.", nameof(label));

        return new ProposalOption
        {
            Id = id ?? Guid.NewGuid(),
            ProposalId = proposalId,
            Label = label.Trim()
        };
    }
}
