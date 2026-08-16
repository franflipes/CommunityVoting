namespace CommunityVoting.Domain.Entities;

public class Community
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Cif { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid VotingSettingsId { get; set; }

    public User CreatedBy { get; set; } = null!;
    public VotingSettings VotingSettings { get; set; } = null!;
    public ICollection<CommunityMember> Members { get; set; } = new List<CommunityMember>();
    public ICollection<Meeting> Meetings { get; set; } = new List<Meeting>();
    public ICollection<CommunityInvitation> Invitations { get; set; } = new List<CommunityInvitation>();

    public Community() { }

    public static Community Create(string name, string address, string? cif, Guid createdByUserId, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("El nombre de la comunidad es obligatorio.", nameof(name));
        if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException("La dirección es obligatoria.", nameof(address));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("El creador de la comunidad es obligatorio.", nameof(createdByUserId));

        var defaultSettings = VotingSettings.CreateDefault();

        return new Community
        {
            Id = id ?? Guid.NewGuid(),
            Name = name.Trim(),
            Address = address.Trim(),
            Cif = cif?.Trim(),
            CreatedByUserId = createdByUserId,
            VotingSettingsId = defaultSettings.Id,
            VotingSettings = defaultSettings
        };
    }
}
