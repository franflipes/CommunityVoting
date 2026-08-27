using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommunityVoting.UnitTests.Application;

public class CommunityServiceTests
{
    private readonly Mock<ICommunityRepository> _communityRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly CommunityService _communityService;

    public CommunityServiceTests()
    {
        _communityRepositoryMock = new Mock<ICommunityRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _communityService = new CommunityService(_communityRepositoryMock.Object, _userRepositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task CreateCommunityAsync_ShouldCreateCommunityAndAddAdminMember()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateCommunityRequest("Residencial Sol", "Calle Sol 1", "H12345678");

        // Act
        var result = await _communityService.CreateCommunityAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(request.Name);
        result.Address.Should().Be(request.Address);
        result.Cif.Should().Be(request.Cif);
        _communityRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Community>()), Times.Once);
    }

    [Fact]
    public async Task GetCommunityByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var communityId = Guid.NewGuid();
        _communityRepositoryMock
            .Setup(r => r.GetByIdAsync(communityId))
            .ReturnsAsync((Community?)null);

        // Act
        var result = await _communityService.GetCommunityByIdAsync(communityId);

        // Assert
        result.Should().BeNull();
    }
}
