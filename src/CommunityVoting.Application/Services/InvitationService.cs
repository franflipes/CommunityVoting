using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.Services;

public class InvitationService
{
    private readonly ICommunityInvitationRepository _invitationRepository;
    private readonly ICommunityRepository _communityRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public InvitationService(
        ICommunityInvitationRepository invitationRepository,
        ICommunityRepository communityRepository,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _invitationRepository = invitationRepository;
        _communityRepository = communityRepository;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<CommunityInvitationDto?> CreateInvitationAsync(CreateInvitationRequest request, Guid createdByUserId, string originUrl)
    {
        var community = await _communityRepository.GetByIdAsync(request.CommunityId);
        if (community == null) return null;

        var invitation = CommunityInvitation.Create(request.CommunityId, createdByUserId, request.ExpiresInDays, request.MaxUses);
        await _invitationRepository.AddAsync(invitation);

        var creator = await _userRepository.GetByIdAsync(createdByUserId);
        var creatorName = creator != null ? $"{creator.Name} {creator.LastName}" : "Administrador";

        var cleanOrigin = originUrl.TrimEnd('/');
        var inviteUrl = $"{cleanOrigin}/join?token={invitation.Token}";

        return new CommunityInvitationDto(
            invitation.Id,
            invitation.CommunityId,
            community.Name,
            invitation.Token,
            inviteUrl,
            invitation.CreatedByUserId,
            creatorName,
            invitation.CreatedAt,
            invitation.ExpiresAt,
            invitation.MaxUses,
            invitation.UsesCount,
            invitation.IsActive,
            invitation.IsValid()
        );
    }

    public async Task<List<CommunityInvitationDto>> GetInvitationsByCommunityAsync(Guid communityId, string originUrl)
    {
        var community = await _communityRepository.GetByIdAsync(communityId);
        var communityName = community?.Name ?? "Comunidad";

        var invitations = await _invitationRepository.GetByCommunityIdAsync(communityId);
        var cleanOrigin = originUrl.TrimEnd('/');

        return invitations.Select(i =>
        {
            var creatorName = i.CreatedByUser != null ? $"{i.CreatedByUser.Name} {i.CreatedByUser.LastName}" : "Administrador";
            var inviteUrl = $"{cleanOrigin}/join?token={i.Token}";
            return new CommunityInvitationDto(
                i.Id,
                i.CommunityId,
                communityName,
                i.Token,
                inviteUrl,
                i.CreatedByUserId,
                creatorName,
                i.CreatedAt,
                i.ExpiresAt,
                i.MaxUses,
                i.UsesCount,
                i.IsActive,
                i.IsValid()
            );
        }).ToList();
    }

    public async Task<VerifyInvitationResponse> VerifyInvitationAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new VerifyInvitationResponse(false, null, null, null, "El token de invitación no es válido.");
        }

        var invitation = await _invitationRepository.GetByTokenAsync(token.Trim());
        if (invitation == null)
        {
            return new VerifyInvitationResponse(false, token, null, null, "El enlace de invitación no existe en el sistema.");
        }

        if (!invitation.IsValid())
        {
            string reason = !invitation.IsActive
                ? "El enlace de invitación ha sido desactivado."
                : (invitation.ExpiresAt.HasValue && invitation.ExpiresAt.Value <= DateTime.UtcNow)
                    ? "El enlace de invitación ha expirado."
                    : "El enlace de invitación ha alcanzado su límite máximo de usos.";

            return new VerifyInvitationResponse(false, token, invitation.CommunityId, invitation.Community?.Name, reason);
        }

        return new VerifyInvitationResponse(true, token, invitation.CommunityId, invitation.Community?.Name, null);
    }

    public async Task<AuthResponse?> RegisterWithInvitationAsync(RegisterWithInvitationRequest request)
    {
        var verify = await VerifyInvitationAsync(request.Token);
        if (!verify.IsValid || !verify.CommunityId.HasValue)
        {
            return null;
        }

        var invitation = await _invitationRepository.GetByTokenAsync(request.Token.Trim());
        if (invitation == null || !invitation.IsValid()) return null;

        // Check if user exists
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLower());
        if (user == null)
        {
            var hashedPassword = _passwordHasher.HashPassword(request.Password);
            user = User.Create(
                request.Name,
                request.LastName,
                request.Email,
                request.PhoneNumber ?? string.Empty,
                hashedPassword,
                UserRole.CommunityMember
            );
            await _userRepository.AddAsync(user);
        }

        // Add to community members if not already member
        var existingMember = await _communityRepository.GetMemberAsync(invitation.CommunityId, user.Id);
        if (existingMember == null)
        {
            var member = CommunityMember.Create(invitation.CommunityId, user.Id, UserRole.CommunityMember);
            await _communityRepository.AddMemberAsync(member);
        }

        // Use invitation token
        invitation.Use();
        await _invitationRepository.UpdateAsync(invitation);

        var token = _jwtProvider.Generate(user);
        var userDto = new UserDto(user.Id, user.Name, user.LastName, user.Email, user.PhoneNumber, user.Role);

        return new AuthResponse(token, userDto);
    }
}
