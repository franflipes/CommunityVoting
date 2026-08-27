using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.Services;

public class MeetingAccessService : IMeetingAccessService
{
    private readonly IMeetingVoterAccessRepository _meetingAccessRepository;
    private readonly IMeetingRepository _meetingRepository;
    private readonly ICommunityRepository _communityRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailOutboxRepository _emailOutboxRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public MeetingAccessService(
        IMeetingVoterAccessRepository meetingAccessRepository,
        IMeetingRepository meetingRepository,
        ICommunityRepository communityRepository,
        IUserRepository userRepository,
        IEmailOutboxRepository emailOutboxRepository,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _meetingAccessRepository = meetingAccessRepository;
        _meetingRepository = meetingRepository;
        _communityRepository = communityRepository;
        _userRepository = userRepository;
        _emailOutboxRepository = emailOutboxRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<List<MeetingAccessCredentialsDto>> CreateAccessForEligibleMembersAsync(Guid meetingId, string baseUrl)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null) return new List<MeetingAccessCredentialsDto>();

        var community = meeting.Community ?? await _communityRepository.GetByIdAsync(meeting.CommunityId);
        var communityName = community?.Name ?? "Comunidad";

        var members = await _communityRepository.GetMembersAsync(meeting.CommunityId);
        var eligibleMembers = members.Where(m => m.IsActive && m.HasVotingRights && m.User != null).ToList();

        var existingAccesses = await _meetingAccessRepository.GetByMeetingIdAsync(meetingId);
        var existingUserIds = existingAccesses.Select(a => a.UserId).ToHashSet();

        var credentialsList = new List<MeetingAccessCredentialsDto>();
        var newAccesses = new List<MeetingVoterAccess>();
        var newOutboxMessages = new List<EmailOutboxMessage>();

        var expiresAt = new DateTimeOffset(meeting.ScheduledAt.AddHours(24), TimeSpan.Zero);

        foreach (var member in eligibleMembers)
        {
            if (existingUserIds.Contains(member.UserId))
            {
                var existing = existingAccesses.First(a => a.UserId == member.UserId);
                var accessUrl = BuildAccessUrl(baseUrl, "existing-token-placeholder");
                credentialsList.Add(new MeetingAccessCredentialsDto(
                    existing.Id,
                    existing.MeetingId,
                    existing.UserId,
                    member.User!.Name,
                    member.User.Email,
                    "[PROTECTED_TOKEN]",
                    "[PROTECTED_CODE]",
                    accessUrl,
                    existing.CreatedAt,
                    existing.ExpiresAt,
                    existing.IsRevoked,
                    existing.LastUsedAt,
                    existing.FailedAttempts
                ));
                continue;
            }

            var (rawToken, tokenHash) = GenerateToken();
            var (rawCode, codeHash) = GenerateCode();

            var access = MeetingVoterAccess.Create(
                meetingId: meetingId,
                userId: member.UserId,
                tokenHash: tokenHash,
                codeHash: codeHash,
                expiresAt: expiresAt
            );

            newAccesses.Add(access);

            var fullAccessUrl = BuildAccessUrl(baseUrl, rawToken);
            credentialsList.Add(new MeetingAccessCredentialsDto(
                access.Id,
                access.MeetingId,
                access.UserId,
                $"{member.User!.Name} {member.User.LastName}".Trim(),
                member.User.Email,
                rawToken,
                rawCode,
                fullAccessUrl,
                access.CreatedAt,
                access.ExpiresAt,
                access.IsRevoked,
                access.LastUsedAt,
                access.FailedAttempts
            ));

            // Create Outbox message payload
            var emailModel = new MeetingInvitationEmailModel
            {
                FirstName = member.User.Name,
                CommunityName = communityName,
                MeetingTitle = meeting.Title,
                MeetingDate = meeting.ScheduledAt.ToString("dd/MM/yyyy"),
                MeetingTime = meeting.ScheduledAt.ToString("HH:mm"),
                AccessUrl = fullAccessUrl,
                AccessCode = rawCode
            };

            var payloadJson = JsonSerializer.Serialize(emailModel);
            var outboxMsg = EmailOutboxMessage.Create(
                type: "MeetingInvitation",
                recipient: member.User.Email,
                template: "Invitation",
                payload: payloadJson,
                meetingId: meetingId,
                userId: member.UserId
            );

            newOutboxMessages.Add(outboxMsg);
        }

        if (newAccesses.Any())
        {
            await _meetingAccessRepository.AddRangeAsync(newAccesses);
        }

        if (newOutboxMessages.Any())
        {
            await _emailOutboxRepository.AddRangeAsync(newOutboxMessages);
        }

        return credentialsList;
    }

    public async Task<MeetingAccessAuthResponse?> AuthenticateAsync(MeetingAccessLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.Code))
        {
            throw new InvalidOperationException("El token y el código son obligatorios.");
        }

        var tokenHash = HashToken(request.Token.Trim());
        var access = await _meetingAccessRepository.GetByTokenHashAsync(tokenHash);

        if (access == null || access.IsRevoked)
        {
            throw new InvalidOperationException("Credencial de acceso no válida o revocada.");
        }

        if (access.ExpiresAt.HasValue && access.ExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("El enlace de acceso ha expirado.");
        }

        if (access.FailedAttempts >= 5)
        {
            throw new InvalidOperationException("La credencial ha sido bloqueada por superar el máximo de intentos fallidos.");
        }

        if (!_passwordHasher.VerifyPassword(request.Code.Trim(), access.CodeHash))
        {
            access.FailedAttempts++;
            if (access.FailedAttempts >= 5)
            {
                access.IsRevoked = true;
            }
            await _meetingAccessRepository.UpdateAsync(access);
            throw new InvalidOperationException("El código de acceso introducido es incorrecto.");
        }

        var meeting = access.Meeting ?? await _meetingRepository.GetByIdAsync(access.MeetingId);
        if (meeting == null)
        {
            throw new InvalidOperationException("La reunión vinculada ya no existe.");
        }

        var members = await _communityRepository.GetMembersAsync(meeting.CommunityId);
        var member = members.FirstOrDefault(m => m.UserId == access.UserId);
        if (member == null || !member.IsActive || !member.HasVotingRights)
        {
            throw new InvalidOperationException("El usuario ya no pertenece a la comunidad o no dispone de derecho a voto activo.");
        }

        var user = access.User ?? await _userRepository.GetByIdAsync(access.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("Usuario no encontrado.");
        }

        access.FailedAttempts = 0;
        access.LastUsedAt = DateTimeOffset.UtcNow;
        await _meetingAccessRepository.UpdateAsync(access);

        var jwtToken = _jwtProvider.Generate(user, authMethod: "meeting_access");
        var userDto = new UserDto(user.Id, user.Name, user.LastName, user.Email, user.PhoneNumber, user.Role);

        return new MeetingAccessAuthResponse(
            AccessToken: jwtToken,
            ExpiresIn: 43200,
            RedirectUrl: $"/meetings/{meeting.Id}",
            User: userDto
        );
    }

    public async Task<List<MeetingAccessCredentialsDto>> GetVoterAccessesForMeetingAsync(Guid meetingId, string baseUrl)
    {
        var accesses = await _meetingAccessRepository.GetByMeetingIdAsync(meetingId);
        var result = new List<MeetingAccessCredentialsDto>();

        foreach (var access in accesses)
        {
            var userName = access.User != null ? $"{access.User.Name} {access.User.LastName}".Trim() : "Usuario";
            var userEmail = access.User?.Email ?? string.Empty;
            var accessUrl = BuildAccessUrl(baseUrl, "token-escondido");

            result.Add(new MeetingAccessCredentialsDto(
                access.Id,
                access.MeetingId,
                access.UserId,
                userName,
                userEmail,
                "[PROTECTED_TOKEN]",
                "[PROTECTED_CODE]",
                accessUrl,
                access.CreatedAt,
                access.ExpiresAt,
                access.IsRevoked,
                access.LastUsedAt,
                access.FailedAttempts
            ));
        }

        return result;
    }

    public async Task<bool> RevokeAccessAsync(Guid meetingId, Guid userId)
    {
        var access = await _meetingAccessRepository.GetByMeetingAndUserAsync(meetingId, userId);
        if (access == null) return false;

        access.IsRevoked = true;
        await _meetingAccessRepository.UpdateAsync(access);
        return true;
    }

    public async Task<MeetingAccessCredentialsDto?> RegenerateAccessAsync(Guid meetingId, Guid userId, string baseUrl)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId);
        if (meeting == null) return null;

        var community = meeting.Community ?? await _communityRepository.GetByIdAsync(meeting.CommunityId);
        var communityName = community?.Name ?? "Comunidad";

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        var existing = await _meetingAccessRepository.GetByMeetingAndUserAsync(meetingId, userId);
        var (rawToken, tokenHash) = GenerateToken();
        var (rawCode, codeHash) = GenerateCode();

        var expiresAt = new DateTimeOffset(meeting.ScheduledAt.AddHours(24), TimeSpan.Zero);

        if (existing != null)
        {
            existing.TokenHash = tokenHash;
            existing.CodeHash = codeHash;
            existing.IsRevoked = false;
            existing.FailedAttempts = 0;
            existing.ExpiresAt = expiresAt;
            existing.CreatedAt = DateTimeOffset.UtcNow;
            await _meetingAccessRepository.UpdateAsync(existing);
        }
        else
        {
            existing = MeetingVoterAccess.Create(meetingId, userId, tokenHash, codeHash, expiresAt);
            await _meetingAccessRepository.AddAsync(existing);
        }

        var fullAccessUrl = BuildAccessUrl(baseUrl, rawToken);

        // Create new Outbox message for regenerated credentials
        var emailModel = new MeetingInvitationEmailModel
        {
            FirstName = user.Name,
            CommunityName = communityName,
            MeetingTitle = meeting.Title,
            MeetingDate = meeting.ScheduledAt.ToString("dd/MM/yyyy"),
            MeetingTime = meeting.ScheduledAt.ToString("HH:mm"),
            AccessUrl = fullAccessUrl,
            AccessCode = rawCode
        };

        var payloadJson = JsonSerializer.Serialize(emailModel);
        var outboxMsg = EmailOutboxMessage.Create(
            type: "MeetingInvitation",
            recipient: user.Email,
            template: "Invitation",
            payload: payloadJson,
            meetingId: meetingId,
            userId: userId
        );

        await _emailOutboxRepository.AddAsync(outboxMsg);

        return new MeetingAccessCredentialsDto(
            existing.Id,
            existing.MeetingId,
            existing.UserId,
            $"{user.Name} {user.LastName}".Trim(),
            user.Email,
            rawToken,
            rawCode,
            fullAccessUrl,
            existing.CreatedAt,
            existing.ExpiresAt,
            existing.IsRevoked,
            existing.LastUsedAt,
            existing.FailedAttempts
        );
    }

    public async Task<List<VoterInvitationStatusDto>> GetInvitationStatusesAsync(Guid meetingId)
    {
        var messages = await _emailOutboxRepository.GetByMeetingIdAsync(meetingId);
        return messages.Select(m => new VoterInvitationStatusDto(
            OutboxId: m.Id,
            MeetingId: m.MeetingId ?? meetingId,
            UserId: m.UserId ?? Guid.Empty,
            UserName: m.User != null ? $"{m.User.Name} {m.User.LastName}".Trim() : "Votante",
            UserEmail: m.Recipient,
            Status: m.Status,
            Attempts: m.Attempts,
            CreatedAt: m.CreatedAt,
            ProcessedAt: m.ProcessedAt,
            LastAttemptAt: m.LastAttemptAt,
            Error: m.Error
        )).ToList();
    }

    public async Task<bool> ResendInvitationAsync(Guid meetingId, Guid userId, string baseUrl)
    {
        var access = await _meetingAccessRepository.GetByMeetingAndUserAsync(meetingId, userId);
        if (access == null || access.IsRevoked) return false;

        var meetingMessages = await _emailOutboxRepository.GetByMeetingIdAsync(meetingId);
        var lastUserMsg = meetingMessages.FirstOrDefault(m => m.UserId == userId);

        if (lastUserMsg != null)
        {
            var newMsg = EmailOutboxMessage.Create(
                type: lastUserMsg.Type,
                recipient: lastUserMsg.Recipient,
                template: lastUserMsg.Template,
                payload: lastUserMsg.Payload,
                meetingId: meetingId,
                userId: userId
            );
            await _emailOutboxRepository.AddAsync(newMsg);
            return true;
        }

        var regen = await RegenerateAccessAsync(meetingId, userId, baseUrl);
        return regen != null;
    }

    public async Task<bool> RetryOutboxMessageAsync(Guid outboxId)
    {
        var msg = await _emailOutboxRepository.GetByIdAsync(outboxId);
        if (msg == null) return false;

        msg.Status = EmailStatus.Pending;
        msg.Attempts = 0;
        msg.NextAttemptAt = null;
        msg.Error = null;

        await _emailOutboxRepository.UpdateAsync(msg);
        return true;
    }

    private static (string rawToken, string tokenHash) GenerateToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToHexString(tokenBytes).ToLowerInvariant();
        var tokenHash = HashToken(rawToken);
        return (rawToken, tokenHash);
    }

    private (string rawCode, string codeHash) GenerateCode()
    {
        var codeInt = RandomNumberGenerator.GetInt32(100000, 1000000);
        var rawCode = codeInt.ToString();
        var codeHash = _passwordHasher.HashPassword(rawCode);
        return (rawCode, codeHash);
    }

    public static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string BuildAccessUrl(string baseUrl, string token)
    {
        var trimmedBase = (baseUrl ?? "http://localhost:5173").TrimEnd('/');
        return $"{trimmedBase}/meeting/access/{token}";
    }
}
