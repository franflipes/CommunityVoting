using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommunityVoting.UnitTests.Application;

public class InvitationServiceTests
{
    private readonly Mock<ICommunityInvitationRepository> _invitationRepositoryMock;
    private readonly Mock<ICommunityRepository> _communityRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly InvitationService _invitationService;

    public InvitationServiceTests()
    {
        _invitationRepositoryMock = new Mock<ICommunityInvitationRepository>();
        _communityRepositoryMock = new Mock<ICommunityRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();

        _invitationService = new InvitationService(
            _invitationRepositoryMock.Object,
            _communityRepositoryMock.Object,
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object);
    }

    [Fact]
    public async Task CreateInvitationAsync_ShouldReturnNull_WhenCommunityDoesNotExist()
    {
        // Arrange
        var request = new CreateInvitationRequest(Guid.NewGuid(), 7, 5);
        _communityRepositoryMock
            .Setup(c => c.GetByIdAsync(request.CommunityId))
            .ReturnsAsync((Community?)null);

        // Act
        var result = await _invitationService.CreateInvitationAsync(request, Guid.NewGuid(), "http://localhost");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task VerifyInvitationAsync_ShouldReturnInvalidResult_WhenTokenNotFound()
    {
        // Arrange
        _invitationRepositoryMock
            .Setup(r => r.GetByTokenAsync("invalid_token"))
            .ReturnsAsync((CommunityInvitation?)null);

        // Act
        var result = await _invitationService.VerifyInvitationAsync("invalid_token");

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("El enlace de invitación no existe en el sistema.");
    }
}
