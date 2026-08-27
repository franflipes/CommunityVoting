using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Interfaces;

public interface IMeetingVoterAccessRepository
{
    Task<MeetingVoterAccess?> GetByMeetingAndUserAsync(Guid meetingId, Guid userId);
    Task<MeetingVoterAccess?> GetByTokenHashAsync(string tokenHash);
    Task<List<MeetingVoterAccess>> GetByMeetingIdAsync(Guid meetingId);
    Task AddAsync(MeetingVoterAccess access);
    Task AddRangeAsync(IEnumerable<MeetingVoterAccess> accesses);
    Task UpdateAsync(MeetingVoterAccess access);
}
