using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IMeetingService
{
    Task<MeetingDto?> CreateMeetingAsync(CreateMeetingRequest request);
    Task<List<MeetingDto>> GetAllMeetingsAsync();
    Task<List<MeetingDto>> GetMeetingsByCommunityAsync(Guid communityId);
    Task<MeetingDto?> GetMeetingDetailsAsync(Guid meetingId);
    Task<VotingEligibleDataDto?> GetVotingEligibleDataAsync(Guid meetingId);
    Task<QuorumStatusDto?> GetQuorumStatusAsync(Guid meetingId);
    Task<MeetingParticipantDto?> RecordAttendanceAsync(Guid meetingId, RecordAttendanceRequest request);
    Task<VotingSettingsDto?> GetVotingSettingsAsync(Guid meetingId);
    Task<VotingSettingsDto?> UpdateVotingSettingsAsync(Guid meetingId, UpdateVotingSettingsRequest request);
    Task<MeetingDto?> UpdateMeetingAsync(Guid meetingId, UpdateMeetingRequest request);
    Task<List<MeetingParticipantDto>> GetParticipantsAsync(Guid meetingId);
    Task<bool> DeleteMeetingAsync(Guid id);
}
