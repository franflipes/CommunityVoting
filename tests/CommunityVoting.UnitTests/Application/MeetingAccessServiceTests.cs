using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommunityVoting.UnitTests.Application;

public class MeetingAccessServiceTests
{
    private readonly Mock<IMeetingVoterAccessRepository> _voterAccessRepositoryMock;
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<ICommunityRepository> _communityRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailOutboxRepository> _emailOutboxRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly MeetingAccessService _meetingAccessService;

    public MeetingAccessServiceTests()
    {
        _voterAccessRepositoryMock = new Mock<IMeetingVoterAccessRepository>();
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _communityRepositoryMock = new Mock<ICommunityRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailOutboxRepositoryMock = new Mock<IEmailOutboxRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtProviderMock = new Mock<IJwtProvider>();

        _meetingAccessService = new MeetingAccessService(
            _voterAccessRepositoryMock.Object,
            _meetingRepositoryMock.Object,
            _communityRepositoryMock.Object,
            _userRepositoryMock.Object,
            _emailOutboxRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtProviderMock.Object);
    }

    [Fact]
    public async Task CreateAccessForEligibleMembersAsync_ShouldReturnEmptyList_WhenMeetingNotFound()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        _meetingRepositoryMock
            .Setup(m => m.GetByIdAsync(meetingId))
            .ReturnsAsync((Meeting?)null);

        // Act
        var result = await _meetingAccessService.CreateAccessForEligibleMembersAsync(meetingId, "http://localhost");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RevokeAccessAsync_ShouldReturnFalse_WhenAccessNotFound()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _voterAccessRepositoryMock
            .Setup(r => r.GetByMeetingAndUserAsync(meetingId, userId))
            .ReturnsAsync((MeetingVoterAccess?)null);

        // Act
        var result = await _meetingAccessService.RevokeAccessAsync(meetingId, userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeAccessAsync_ShouldRevokeAndReturnTrue_WhenAccessExists()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var access = MeetingVoterAccess.Create(meetingId, userId, "thash", "chash", DateTimeOffset.UtcNow.AddDays(1));

        _voterAccessRepositoryMock
            .Setup(r => r.GetByMeetingAndUserAsync(meetingId, userId))
            .ReturnsAsync(access);

        // Act
        var result = await _meetingAccessService.RevokeAccessAsync(meetingId, userId);

        // Assert
        result.Should().BeTrue();
        access.IsRevoked.Should().BeTrue();
        _voterAccessRepositoryMock.Verify(r => r.UpdateAsync(access), Times.Once);
    }
}
