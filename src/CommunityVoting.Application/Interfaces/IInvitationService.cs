using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IInvitationService
{
    Task<CommunityInvitationDto?> CreateInvitationAsync(CreateInvitationRequest request, Guid createdByUserId, string originUrl);
    Task<List<CommunityInvitationDto>> GetInvitationsByCommunityAsync(Guid communityId, string originUrl);
    Task<VerifyInvitationResponse> VerifyInvitationAsync(string token);
    Task<AuthResponse?> RegisterWithInvitationAsync(RegisterWithInvitationRequest request);
}
