using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IProposalService
{
    Task<ProposalDto?> CreateProposalAsync(CreateProposalRequest request);
    Task<ProposalDto?> UpdateMajoritySettingsAsync(Guid proposalId, UpdateProposalMajorityRequest request);
    Task<ProposalDto?> UpdateProposalAsync(Guid proposalId, UpdateProposalRequest request);
    Task<ProposalDto?> GetProposalByIdAsync(Guid proposalId);
    Task<ProposalOptionDto?> AddOptionAsync(CreateProposalOptionRequest request);
    Task<bool> DeleteOptionAsync(Guid optionId);
    Task<bool> DeleteProposalAsync(Guid proposalId);
}
