using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;

namespace CommunityVoting.Application.Services;

public class MeetingService
{
    private readonly IMeetingRepository _meetingRepository;
    private readonly ICommunityRepository _communityRepository;

    public MeetingService(IMeetingRepository meetingRepository, ICommunityRepository communityRepository)
    {
        _meetingRepository = meetingRepository;
        _communityRepository = communityRepository;
    }

    public async Task<MeetingDto?> CreateMeetingAsync(CreateMeetingRequest request)
    {
        var community = await _communityRepository.GetByIdAsync(request.CommunityId);
        if (community == null) return null;

        var meeting = Meeting.Create(
            request.CommunityId,
            request.Title,
            request.Type,
            request.Location,
            request.ScheduledAt,
            request.SecondCallAt,
            request.IsTransparent,
            communitySettings: community.VotingSettings
        );

        await _meetingRepository.AddAsync(meeting);

        var vsDto = meeting.VotingSettings != null ? MapVotingSettingsDto(meeting.VotingSettings) : null;

        return new MeetingDto(
            meeting.Id,
            meeting.CommunityId,
            community.Name,
            meeting.Title,
            meeting.Type,
            meeting.Location,
            meeting.ScheduledAt,
            meeting.SecondCallAt,
            meeting.VotingStart,
            meeting.VotingEnd,
            meeting.IsTransparent,
            vsDto,
            new List<AgendaItemDto>(),
            new List<ProposalDto>()
        );
    }

    public async Task<List<MeetingDto>> GetMeetingsByCommunityAsync(Guid communityId)
    {
        var community = await _communityRepository.GetByIdAsync(communityId);
        var communityName = community?.Name ?? "Comunidad";

        var meetings = await _meetingRepository.GetByCommunityIdAsync(communityId);
        return meetings.Select(m => new MeetingDto(
            m.Id,
            m.CommunityId,
            communityName,
            m.Title,
            m.Type,
            m.Location,
            m.ScheduledAt,
            m.SecondCallAt,
            m.VotingStart,
            m.VotingEnd,
            m.IsTransparent,
            m.VotingSettings != null ? MapVotingSettingsDto(m.VotingSettings) : null,
            new List<AgendaItemDto>(),
            new List<ProposalDto>()
        )).ToList();
    }

    public async Task<MeetingDto?> GetMeetingDetailsAsync(Guid meetingId)
    {
        var m = await _meetingRepository.GetByIdWithProposalsAsync(meetingId);
        if (m == null) return null;

        var agendaItemDtos = m.AgendaItems.Select(AgendaItemService.MapToDto).ToList();
        var proposalDtos = m.Proposals.Select(ProposalService.MapToDto).ToList();
        var vsDto = m.VotingSettings != null ? MapVotingSettingsDto(m.VotingSettings) : null;

        return new MeetingDto(
            m.Id,
            m.CommunityId,
            m.Community?.Name ?? "Comunidad",
            m.Title,
            m.Type,
            m.Location,
            m.ScheduledAt,
            m.SecondCallAt,
            m.VotingStart,
            m.VotingEnd,
            m.IsTransparent,
            vsDto,
            agendaItemDtos,
            proposalDtos
        );
    }

    public async Task<VotingEligibleDataDto?> GetVotingEligibleDataAsync(Guid meetingId)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null) return null;

        var members = await _communityRepository.GetMembersAsync(meeting.CommunityId);
        var eligibleMembersList = members
            .Where(m => m.User != null && m.IsActive && m.HasVotingRights)
            .ToList();

        var eligibleVoters = eligibleMembersList
            .Select(m => new EligibleVoterDto(
                m.UserId,
                m.User!.Name,
                m.User.LastName,
                m.User.Email,
                m.User.PhoneNumber,
                m.MemberRole
            )).ToList();

        var eligibleUserIds = eligibleMembersList.Select(m => m.UserId).ToHashSet();
        var participants = await _meetingRepository.GetParticipantsAsync(meetingId);
        int presentCount = participants.Count(p => p.IsPresent && eligibleUserIds.Contains(p.UserId));
        int totalEligible = eligibleMembersList.Count;

        decimal quorumPct = meeting.VotingSettings?.QuorumPercentage ?? 50.0m;
        bool quorumEnabled = meeting.VotingSettings?.QuorumEnabled ?? true;
        int quorumReq = quorumEnabled ? (int)Math.Ceiling(totalEligible * (quorumPct / 100.0m)) : 0;
        bool quorumReached = !quorumEnabled || presentCount >= quorumReq;

        return new VotingEligibleDataDto(
            meeting.CommunityId,
            meeting.Id,
            meeting.Title,
            meeting.ScheduledAt,
            totalEligible,
            presentCount,
            quorumReq,
            quorumReached,
            eligibleVoters
        );
    }

    public async Task<QuorumStatusDto?> GetQuorumStatusAsync(Guid meetingId)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null) return null;

        var members = await _communityRepository.GetMembersAsync(meeting.CommunityId);
        var eligibleUserIds = members
            .Where(m => m.User != null && m.IsActive && m.HasVotingRights)
            .Select(m => m.UserId)
            .ToHashSet();

        var participants = await _meetingRepository.GetParticipantsAsync(meetingId);
        int presentCount = participants.Count(p => p.IsPresent && eligibleUserIds.Contains(p.UserId));
        int totalEligible = eligibleUserIds.Count;

        decimal quorumPct = meeting.VotingSettings?.QuorumPercentage ?? 50.0m;
        bool quorumEnabled = meeting.VotingSettings?.QuorumEnabled ?? true;
        bool requireQuorum = meeting.VotingSettings?.RequireQuorumForVoting ?? true;
        int quorumReq = quorumEnabled ? (int)Math.Ceiling(totalEligible * (quorumPct / 100.0m)) : 0;
        bool quorumReached = !quorumEnabled || presentCount >= quorumReq;

        return new QuorumStatusDto(
            meeting.Id,
            totalEligible,
            presentCount,
            quorumReq,
            quorumPct,
            quorumReached,
            requireQuorum
        );
    }

    public async Task<MeetingParticipantDto?> RecordAttendanceAsync(Guid meetingId, RecordAttendanceRequest request)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null) return null;

        var participant = MeetingParticipant.Create(meetingId, request.UserId, request.IsPresent);
        await _meetingRepository.AddParticipantAsync(participant);

        var updated = await _meetingRepository.GetParticipantAsync(meetingId, request.UserId);
        if (updated == null) return null;

        return new MeetingParticipantDto(
            updated.Id,
            updated.MeetingId,
            updated.UserId,
            updated.User?.Name ?? string.Empty,
            updated.User?.LastName ?? string.Empty,
            updated.User?.Email ?? string.Empty,
            updated.JoinedAt,
            updated.IsPresent
        );
    }

    public async Task<VotingSettingsDto?> GetVotingSettingsAsync(Guid meetingId)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null || meeting.VotingSettings == null) return null;
        return MapVotingSettingsDto(meeting.VotingSettings);
    }

    public async Task<VotingSettingsDto?> UpdateVotingSettingsAsync(Guid meetingId, UpdateVotingSettingsRequest request)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null || meeting.VotingSettings == null) return null;

        var s = meeting.VotingSettings;
        s.QuorumEnabled = request.QuorumEnabled;
        s.QuorumType = request.QuorumType;
        s.QuorumPercentage = request.QuorumPercentage;
        s.RequireQuorumForVoting = request.RequireQuorumForVoting;
        s.DefaultMajorityType = request.DefaultMajorityType;
        s.DefaultMajorityPercentage = request.DefaultMajorityPercentage;
        s.AbstentionPolicy = request.AbstentionPolicy;

        await _meetingRepository.UpdateVotingSettingsAsync(s);
        return MapVotingSettingsDto(s);
    }

    public async Task<MeetingDto?> UpdateMeetingAsync(Guid meetingId, UpdateMeetingRequest request)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null) return null;

        meeting.Update(
            request.Title,
            request.Type,
            request.Location,
            request.ScheduledAt,
            request.SecondCallAt,
            request.IsTransparent
        );

        await _meetingRepository.UpdateAsync(meeting);
        return await GetMeetingDetailsAsync(meetingId);
    }

    public async Task<List<MeetingParticipantDto>> GetParticipantsAsync(Guid meetingId)
    {
        var participants = await _meetingRepository.GetParticipantsAsync(meetingId);
        return participants.Select(p => new MeetingParticipantDto(
            p.Id,
            p.MeetingId,
            p.UserId,
            p.User?.Name ?? string.Empty,
            p.User?.LastName ?? string.Empty,
            p.User?.Email ?? string.Empty,
            p.JoinedAt,
            p.IsPresent
        )).ToList();
    }

    public async Task<bool> DeleteMeetingAsync(Guid id)
    {
        var meeting = await _meetingRepository.GetByIdAsync(id);
        if (meeting == null) return false;
        await _meetingRepository.DeleteAsync(id);
        return true;
    }

    private static VotingSettingsDto MapVotingSettingsDto(VotingSettings s)
    {
        return new VotingSettingsDto(
            s.Id,
            s.QuorumEnabled,
            s.QuorumType,
            s.QuorumPercentage,
            s.RequireQuorumForVoting,
            s.DefaultMajorityType,
            s.DefaultMajorityPercentage,
            s.AbstentionPolicy
        );
    }
}
