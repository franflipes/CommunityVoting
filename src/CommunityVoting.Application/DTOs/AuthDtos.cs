using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.DTOs;

public record RegisterRequest(
    string Name,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber,
    UserRole Role = UserRole.CommunityMember
);

public record LoginRequest(
    string Email,
    string Password
);

public record UserDto(
    Guid Id,
    string Name,
    string LastName,
    string Email,
    string PhoneNumber,
    UserRole Role
);

public record AuthResponse(
    string Token,
    UserDto User
);
