using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;

namespace CommunityVoting.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing != null) return null;

        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        var user = User.Create(
            request.Name,
            request.LastName,
            request.Email,
            request.PhoneNumber ?? string.Empty,
            hashedPassword,
            request.Role
        );

        await _userRepository.AddAsync(user);

        var token = _jwtProvider.Generate(user);
        var userDto = new UserDto(user.Id, user.Name, user.LastName, user.Email, user.PhoneNumber, user.Role);

        return new AuthResponse(token, userDto);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtProvider.Generate(user);
        var userDto = new UserDto(user.Id, user.Name, user.LastName, user.Email, user.PhoneNumber, user.Role);

        return new AuthResponse(token, userDto);
    }

    public async Task<UserDto?> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;
        return new UserDto(user.Id, user.Name, user.LastName, user.Email, user.PhoneNumber, user.Role);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) return false;

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null || !_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userRepository.UpdateAsync(user);
        return true;
    }
}
