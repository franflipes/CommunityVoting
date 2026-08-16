using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public ICollection<CommunityMember> Memberships { get; set; } = new List<CommunityMember>();

    public User() { }

    public static User Create(string name, string lastName, string email, string phoneNumber, string passwordHash, UserRole role = UserRole.CommunityMember, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("El nombre es obligatorio.", nameof(name));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Los apellidos son obligatorios.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("El email es obligatorio.", nameof(email));

        return new User
        {
            Id = id ?? Guid.NewGuid(),
            Name = name.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLower(),
            PhoneNumber = phoneNumber?.Trim() ?? string.Empty,
            PasswordHash = passwordHash,
            Role = role
        };
    }
}
