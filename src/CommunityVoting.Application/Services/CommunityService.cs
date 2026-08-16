using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.Services;

public class CommunityService
{
    private readonly ICommunityRepository _communityRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CommunityService(ICommunityRepository communityRepository, IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _communityRepository = communityRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<CommunityDto> CreateCommunityAsync(CreateCommunityRequest request, Guid createdByUserId)
    {
        var community = Community.Create(request.Name, request.Address, request.Cif, createdByUserId);
        await _communityRepository.AddAsync(community);

        // Creator becomes CommunityAdmin member automatically
        var adminMember = CommunityMember.Create(community.Id, createdByUserId, UserRole.CommunityAdmin);
        await _communityRepository.AddMemberAsync(adminMember);

        var creator = await _userRepository.GetByIdAsync(createdByUserId);
        var creatorName = creator != null ? $"{creator.Name} {creator.LastName}" : "Administrador";

        return new CommunityDto(
            community.Id,
            community.Name,
            community.Address,
            community.Cif,
            community.CreatedByUserId,
            creatorName,
            1,
            0
        );
    }

    public async Task<List<CommunityDto>> GetCommunitiesForUserAsync(Guid userId, UserRole userRole)
    {
        List<Community> communities;
        if (userRole == UserRole.GlobalAdmin)
        {
            communities = await _communityRepository.GetAllAsync();
        }
        else
        {
            communities = await _communityRepository.GetByUserIdAsync(userId);
        }

        var result = new List<CommunityDto>();
        foreach (var c in communities)
        {
            var members = await _communityRepository.GetMembersAsync(c.Id);
            var creatorName = c.CreatedBy != null ? $"{c.CreatedBy.Name} {c.CreatedBy.LastName}" : "Administrador";
            result.Add(new CommunityDto(
                c.Id,
                c.Name,
                c.Address,
                c.Cif,
                c.CreatedByUserId,
                creatorName,
                members.Count,
                c.Meetings?.Count ?? 0
            ));
        }

        return result;
    }

    public async Task<CommunityDto?> GetCommunityByIdAsync(Guid communityId)
    {
        var c = await _communityRepository.GetByIdAsync(communityId);
        if (c == null) return null;

        var members = await _communityRepository.GetMembersAsync(c.Id);
        var creatorName = c.CreatedBy != null ? $"{c.CreatedBy.Name} {c.CreatedBy.LastName}" : "Administrador";

        return new CommunityDto(
            c.Id,
            c.Name,
            c.Address,
            c.Cif,
            c.CreatedByUserId,
            creatorName,
            members.Count,
            c.Meetings?.Count ?? 0
        );
    }

    public async Task<CommunityMemberDto?> AddMemberAsync(Guid communityId, AddCommunityMemberRequest request)
    {
        var community = await _communityRepository.GetByIdAsync(communityId);
        if (community == null) return null;

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            var newUserId = Guid.NewGuid();
            var dummyPasswordHash = _passwordHasher.HashPassword("Voter123!");
            user = User.Create(
                request.Name,
                request.LastName ?? string.Empty,
                request.Email,
                request.PhoneNumber ?? string.Empty,
                dummyPasswordHash,
                UserRole.CommunityMember,
                newUserId
            );
            await _userRepository.AddAsync(user);
        }

        var member = CommunityMember.Create(communityId, user.Id, request.MemberRole);
        await _communityRepository.AddMemberAsync(member);

        return new CommunityMemberDto(
            member.Id,
            member.CommunityId,
            member.UserId,
            user.Name,
            user.LastName,
            user.Email,
            member.MemberRole,
            member.HasVotingRights,
            member.IsActive,
            member.JoinedAt
        );
    }

    public async Task<List<CommunityMemberDto>> GetMembersAsync(Guid communityId)
    {
        var members = await _communityRepository.GetMembersAsync(communityId);
        return members.Select(m => new CommunityMemberDto(
            m.Id,
            m.CommunityId,
            m.UserId,
            m.User?.Name ?? string.Empty,
            m.User?.LastName ?? string.Empty,
            m.User?.Email ?? string.Empty,
            m.MemberRole,
            m.HasVotingRights,
            m.IsActive,
            m.JoinedAt
        )).ToList();
    }

    public async Task<VotingSettingsDto?> GetVotingSettingsAsync(Guid communityId)
    {
        var community = await _communityRepository.GetByIdAsync(communityId);
        if (community == null || community.VotingSettings == null) return null;

        var s = community.VotingSettings;
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

    public async Task<VotingSettingsDto?> UpdateVotingSettingsAsync(Guid communityId, UpdateVotingSettingsRequest request)
    {
        var community = await _communityRepository.GetByIdAsync(communityId);
        if (community == null || community.VotingSettings == null) return null;

        var s = community.VotingSettings;
        s.QuorumEnabled = request.QuorumEnabled;
        s.QuorumType = request.QuorumType;
        s.QuorumPercentage = request.QuorumPercentage;
        s.RequireQuorumForVoting = request.RequireQuorumForVoting;
        s.DefaultMajorityType = request.DefaultMajorityType;
        s.DefaultMajorityPercentage = request.DefaultMajorityPercentage;
        s.AbstentionPolicy = request.AbstentionPolicy;

        await _communityRepository.UpdateVotingSettingsAsync(s);

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

    public async Task<CommunityMemberDto?> UpdateMemberVotingRightsAsync(Guid communityId, Guid userId, UpdateMemberVotingRightsRequest request)
    {
        var member = await _communityRepository.GetMemberAsync(communityId, userId);
        if (member == null) return null;

        member.HasVotingRights = request.HasVotingRights;
        member.IsActive = request.IsActive;

        await _communityRepository.UpdateMemberAsync(member);

        return new CommunityMemberDto(
            member.Id,
            member.CommunityId,
            member.UserId,
            member.User?.Name ?? string.Empty,
            member.User?.LastName ?? string.Empty,
            member.User?.Email ?? string.Empty,
            member.MemberRole,
            member.HasVotingRights,
            member.IsActive,
            member.JoinedAt
        );
    }
}
