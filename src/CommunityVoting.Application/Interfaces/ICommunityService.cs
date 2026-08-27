using CommunityVoting.Application.DTOs;
using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.Interfaces;

public interface ICommunityService
{
    Task<CommunityDto> CreateCommunityAsync(CreateCommunityRequest request, Guid createdByUserId);
    Task<List<CommunityDto>> GetCommunitiesForUserAsync(Guid userId, UserRole userRole);
    Task<CommunityDto?> GetCommunityByIdAsync(Guid communityId);
    Task<CommunityMemberDto?> AddMemberAsync(Guid communityId, AddCommunityMemberRequest request);
    Task<List<CommunityMemberDto>> GetMembersAsync(Guid communityId);
    Task<VotingSettingsDto?> GetVotingSettingsAsync(Guid communityId);
    Task<VotingSettingsDto?> UpdateVotingSettingsAsync(Guid communityId, UpdateVotingSettingsRequest request);
    Task<CommunityMemberDto?> UpdateMemberVotingRightsAsync(Guid communityId, Guid userId, UpdateMemberVotingRightsRequest request);
}
