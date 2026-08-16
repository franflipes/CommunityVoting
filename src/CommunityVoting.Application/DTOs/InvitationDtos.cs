using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.DTOs;

public record CreateInvitationRequest(
    Guid CommunityId,
    int? ExpiresInDays = 7,
    int MaxUses = 0
);

public record CommunityInvitationDto(
    Guid Id,
    Guid CommunityId,
    string CommunityName,
    string Token,
    string InviteUrl,
    Guid CreatedByUserId,
    string CreatedByUserName,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    int MaxUses,
    int UsesCount,
    bool IsActive,
    bool IsValid
);

public record VerifyInvitationResponse(
    bool IsValid,
    string? Token,
    Guid? CommunityId,
    string? CommunityName,
    string? ErrorMessage
);

public record RegisterWithInvitationRequest(
    string Token,
    string Name,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber
);
