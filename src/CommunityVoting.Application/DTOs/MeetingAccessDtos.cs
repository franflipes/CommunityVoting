using System;

namespace CommunityVoting.Application.DTOs;

public record MeetingAccessLoginRequest(
    string Token,
    string Code
);

public record MeetingAccessAuthResponse(
    string AccessToken,
    int ExpiresIn,
    string RedirectUrl,
    UserDto User
);

public record MeetingAccessCredentialsDto(
    Guid Id,
    Guid MeetingId,
    Guid UserId,
    string UserName,
    string UserEmail,
    string Token,
    string Code,
    string AccessUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    bool IsRevoked,
    DateTimeOffset? LastUsedAt,
    int FailedAttempts
);

public record MeetingVoterAccessSummaryDto(
    Guid Id,
    Guid MeetingId,
    Guid UserId,
    string UserName,
    string UserEmail,
    string AccessUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    bool IsRevoked,
    DateTimeOffset? LastUsedAt,
    int FailedAttempts
);
