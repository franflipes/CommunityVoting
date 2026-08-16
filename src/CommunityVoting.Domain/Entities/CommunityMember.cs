using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Domain.Entities;

public class CommunityMember
{
    public Guid Id { get; set; }
    public Guid CommunityId { get; set; }
    public Guid UserId { get; set; }
    public UserRole MemberRole { get; set; } = UserRole.CommunityMember;
    public bool HasVotingRights { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public Community Community { get; set; } = null!;
    public User User { get; set; } = null!;

    public CommunityMember() { }

    public static CommunityMember Create(Guid communityId, Guid userId, UserRole role = UserRole.CommunityMember, Guid? id = null)
    {
        if (communityId == Guid.Empty) throw new ArgumentException("El ID de la comunidad es obligatorio.", nameof(communityId));
        if (userId == Guid.Empty) throw new ArgumentException("El ID del usuario es obligatorio.", nameof(userId));

        return new CommunityMember
        {
            Id = id ?? Guid.NewGuid(),
            CommunityId = communityId,
            UserId = userId,
            MemberRole = role,
            JoinedAt = DateTime.UtcNow
        };
    }
}
