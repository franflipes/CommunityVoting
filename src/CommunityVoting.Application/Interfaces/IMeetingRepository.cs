using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Interfaces;

public interface IMeetingRepository
{
    Task<Meeting?> GetByIdAsync(Guid id);
    Task<Meeting?> GetByIdWithProposalsAsync(Guid id);
    Task<List<Meeting>> GetAllAsync();
    Task<List<Meeting>> GetByCommunityIdAsync(Guid communityId);
    Task AddAsync(Meeting meeting);
    Task UpdateAsync(Meeting meeting);
    Task DeleteAsync(Guid id);
    Task AddParticipantAsync(MeetingParticipant participant);
    Task<MeetingParticipant?> GetParticipantAsync(Guid meetingId, Guid userId);
    Task<List<MeetingParticipant>> GetParticipantsAsync(Guid meetingId);
    Task UpdateVotingSettingsAsync(VotingSettings settings);
}
