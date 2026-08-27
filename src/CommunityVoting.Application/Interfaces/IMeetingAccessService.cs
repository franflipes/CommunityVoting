using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IMeetingAccessService
{
    Task<List<MeetingAccessCredentialsDto>> CreateAccessForEligibleMembersAsync(Guid meetingId, string baseUrl);
    Task<MeetingAccessAuthResponse?> AuthenticateAsync(MeetingAccessLoginRequest request);
    Task<List<MeetingAccessCredentialsDto>> GetVoterAccessesForMeetingAsync(Guid meetingId, string baseUrl);
    Task<bool> RevokeAccessAsync(Guid meetingId, Guid userId);
    Task<MeetingAccessCredentialsDto?> RegenerateAccessAsync(Guid meetingId, Guid userId, string baseUrl);
    Task<List<VoterInvitationStatusDto>> GetInvitationStatusesAsync(Guid meetingId);
    Task<bool> ResendInvitationAsync(Guid meetingId, Guid userId, string baseUrl);
    Task<bool> RetryOutboxMessageAsync(Guid outboxId);
}
