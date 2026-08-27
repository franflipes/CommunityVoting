using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommunityVoting.UnitTests.Application;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _authService = new AuthService(_userRepositoryMock.Object, _passwordHasherMock.Object, _jwtProviderMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnNull_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest("Carlos", "Gomez", "carlos@example.com", "123456", "Password123!");
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(User.Create("Carlos", "Gomez", request.Email, "123", "hash", UserRole.CommunityMember));

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnToken_WhenEmailIsUnique()
    {
        // Arrange
        var request = new RegisterRequest("Carlos", "Gomez", "newuser@example.com", "+34600", "Pass123!");
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _passwordHasherMock
            .Setup(p => p.HashPassword(request.Password))
            .Returns("hashed_pass");

        _jwtProviderMock
            .Setup(j => j.Generate(It.IsAny<User>(), null))
            .Returns("fake_jwt_token");

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("fake_jwt_token");
        result.User.Email.Should().Be(request.Email);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var request = new LoginRequest("notfound@example.com", "password");
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsInvalid()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "wrong_password");
        var user = User.Create("Carlos", "Gomez", request.Email, "123", "hashed_pass", UserRole.CommunityMember);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreCorrect()
    {
        // Arrange
        var request = new LoginRequest("valid@example.com", "correct_pass");
        var user = User.Create("Carlos", "Gomez", request.Email, "123", "hashed_pass", UserRole.CommunityMember);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        _jwtProviderMock
            .Setup(j => j.Generate(user, null))
            .Returns("valid_jwt");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("valid_jwt");
        result.User.Email.Should().Be(request.Email);
    }
}
