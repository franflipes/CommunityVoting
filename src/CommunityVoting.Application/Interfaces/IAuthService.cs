using CommunityVoting.Application.DTOs;

namespace CommunityVoting.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<UserDto?> GetProfileAsync(Guid userId);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
