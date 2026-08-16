using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Interfaces;

public interface ICommunityRepository
{
    Task<Community?> GetByIdAsync(Guid id);
    Task<List<Community>> GetAllAsync();
    Task<List<Community>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Community community);
    Task AddMemberAsync(CommunityMember member);
    Task UpdateAsync(Community community);
    Task UpdateMemberAsync(CommunityMember member);
    Task UpdateVotingSettingsAsync(VotingSettings settings);
    Task<CommunityMember?> GetMemberAsync(Guid communityId, Guid userId);
    Task<List<CommunityMember>> GetMembersAsync(Guid communityId);
}
