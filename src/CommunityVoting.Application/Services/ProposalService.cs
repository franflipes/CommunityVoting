using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Services;

public class ProposalService : IProposalService
{
    private readonly IProposalRepository _proposalRepository;
    private readonly IMeetingRepository _meetingRepository;

    public ProposalService(IProposalRepository proposalRepository, IMeetingRepository meetingRepository)
    {
        _proposalRepository = proposalRepository;
        _meetingRepository = meetingRepository;
    }

    public async Task<ProposalDto?> CreateProposalAsync(CreateProposalRequest request)
    {
        var meeting = await _meetingRepository.GetByIdAsync(request.MeetingId);
        if (meeting == null) return null;

        var proposal = Proposal.Create(
            request.MeetingId,
            request.AgendaItemId,
            request.Title,
            request.Description,
            request.Order,
            request.MajorityType,
            request.MajorityPercentage
        );
        await _proposalRepository.AddAsync(proposal);

        var optionsList = request.Options != null && request.Options.Any()
            ? request.Options
            : new List<string> { "A favor", "En contra", "Abstención" };

        foreach (var label in optionsList)
        {
            var opt = ProposalOption.Create(proposal.Id, label);
            await _proposalRepository.AddOptionAsync(opt);
            proposal.Options.Add(opt);
        }

        return MapToDto(proposal);
    }

    public async Task<ProposalDto?> UpdateMajoritySettingsAsync(Guid proposalId, UpdateProposalMajorityRequest request)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId);
        if (proposal == null) return null;

        proposal.MajorityType = request.MajorityType;
        proposal.MajorityPercentage = request.MajorityPercentage;

        await _proposalRepository.UpdateAsync(proposal);
        return MapToDto(proposal);
    }

    public async Task<ProposalDto?> UpdateProposalAsync(Guid proposalId, UpdateProposalRequest request)
    {
        var proposal = await _proposalRepository.GetByIdAsync(proposalId);
        if (proposal == null) return null;

        proposal.Title = request.Title;
        proposal.Description = request.Description ?? string.Empty;
        proposal.Order = request.Order;
        proposal.MajorityType = request.MajorityType;
        proposal.MajorityPercentage = request.MajorityPercentage;

        if (request.Options != null && request.Options.Any())
        {
            foreach (var opt in proposal.Options.ToList())
            {
                await _proposalRepository.DeleteOptionAsync(opt.Id);
            }
            proposal.Options.Clear();

            foreach (var label in request.Options)
            {
                var newOpt = ProposalOption.Create(proposal.Id, label);
                await _proposalRepository.AddOptionAsync(newOpt);
                proposal.Options.Add(newOpt);
            }
        }

        await _proposalRepository.UpdateAsync(proposal);
        return MapToDto(proposal);
    }

    public async Task<ProposalDto?> GetProposalByIdAsync(Guid proposalId)
    {
        var p = await _proposalRepository.GetByIdAsync(proposalId);
        if (p == null) return null;
        return MapToDto(p);
    }

    public async Task<ProposalOptionDto?> AddOptionAsync(CreateProposalOptionRequest request)
    {
        var proposal = await _proposalRepository.GetByIdAsync(request.ProposalId);
        if (proposal == null) return null;

        var option = ProposalOption.Create(request.ProposalId, request.Label);
        await _proposalRepository.AddOptionAsync(option);

        return new ProposalOptionDto(option.Id, option.ProposalId, option.Label);
    }

    public async Task<bool> DeleteOptionAsync(Guid optionId)
    {
        await _proposalRepository.DeleteOptionAsync(optionId);
        return true;
    }

    public async Task<bool> DeleteProposalAsync(Guid proposalId)
    {
        var p = await _proposalRepository.GetByIdAsync(proposalId);
        if (p == null) return false;
        await _proposalRepository.DeleteAsync(proposalId);
        return true;
    }

    public static ProposalDto MapToDto(Proposal p)
    {
        return new ProposalDto(
            p.Id,
            p.MeetingId,
            p.AgendaItemId,
            p.Title,
            p.Description,
            p.Order,
            p.MajorityType,
            p.MajorityPercentage,
            p.Options?.Select(o => new ProposalOptionDto(o.Id, o.ProposalId, o.Label)).ToList() ?? new List<ProposalOptionDto>(),
            p.Documents?.Select(d => new DocumentDto(
                d.Id,
                d.ProposalId,
                d.Title,
                d.Description,
                d.FileName,
                d.ContentType,
                d.FileSize,
                d.UploadedByUserId,
                d.UploadedByUser != null ? $"{d.UploadedByUser.Name} {d.UploadedByUser.LastName}" : "Usuario",
                d.UploadedAt
            )).ToList() ?? new List<DocumentDto>()
        );
    }
}
