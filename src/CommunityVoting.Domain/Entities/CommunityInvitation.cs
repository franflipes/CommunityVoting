using System.Security.Cryptography;

namespace CommunityVoting.Domain.Entities;

public class CommunityInvitation
{
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public string Token { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public int MaxUses { get; set; } = 0; // 0 = unlimited
    public int UsesCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public Community Community { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;

    public CommunityInvitation() { }

    public static CommunityInvitation Create(Guid communityId, Guid createdByUserId, int? expiresInDays = 7, int maxUses = 0, Guid? id = null)
    {
        if (communityId == Guid.Empty) throw new ArgumentException("Comunidad requerida.", nameof(communityId));
        if (createdByUserId == Guid.Empty) throw new ArgumentException("Creador requerido.", nameof(createdByUserId));

        // Generate secure random token
        var randomBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        var token = Convert.ToHexString(randomBytes).ToLowerInvariant();

        return new CommunityInvitation
        {
            Id = id ?? Guid.NewGuid(),
            CommunityId = communityId,
            Token = token,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresInDays.HasValue && expiresInDays.Value > 0 ? DateTime.UtcNow.AddDays(expiresInDays.Value) : null,
            MaxUses = maxUses,
            UsesCount = 0,
            IsActive = true
        };
    }

    public bool IsValid()
    {
        if (!IsActive) return false;
        if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow) return false;
        if (MaxUses > 0 && UsesCount >= MaxUses) return false;
        return true;
    }

    public void Use()
    {
        UsesCount++;
        if (MaxUses > 0 && UsesCount >= MaxUses)
        {
            IsActive = false;
        }
    }
}
