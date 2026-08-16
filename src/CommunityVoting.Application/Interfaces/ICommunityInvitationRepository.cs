using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Interfaces;

public interface ICommunityInvitationRepository
{
    Task AddAsync(CommunityInvitation invitation);
    Task<CommunityInvitation?> GetByTokenAsync(string token);
    Task<List<CommunityInvitation>> GetByCommunityIdAsync(Guid communityId);
    Task UpdateAsync(CommunityInvitation invitation);
}
