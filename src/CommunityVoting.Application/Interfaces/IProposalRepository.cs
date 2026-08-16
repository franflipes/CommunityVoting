using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Interfaces;

public interface IProposalRepository
{
    Task<Proposal?> GetByIdAsync(Guid id);
    Task<List<Proposal>> GetByMeetingIdAsync(Guid meetingId);
    Task AddAsync(Proposal proposal);
    Task UpdateAsync(Proposal proposal);
    Task DeleteAsync(Guid id);
    Task AddOptionAsync(ProposalOption option);
    Task DeleteOptionAsync(Guid optionId);
}
