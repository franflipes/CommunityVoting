using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommunityVoting.UnitTests.Application;

public class MeetingServiceTests
{
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly Mock<ICommunityRepository> _communityRepositoryMock;
    private readonly Mock<IMeetingAccessService> _meetingAccessServiceMock;
    private readonly MeetingService _meetingService;

    public MeetingServiceTests()
    {
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _communityRepositoryMock = new Mock<ICommunityRepository>();
        _meetingAccessServiceMock = new Mock<IMeetingAccessService>();

        _meetingService = new MeetingService(
            _meetingRepositoryMock.Object,
            _communityRepositoryMock.Object,
            _meetingAccessServiceMock.Object);
    }

    [Fact]
    public async Task CreateMeetingAsync_ShouldReturnNull_WhenCommunityDoesNotExist()
    {
        // Arrange
        var request = new CreateMeetingRequest(
            Guid.NewGuid(),
            "Junta 2026",
            MeetingType.Ordinary,
            "Online",
            DateTime.UtcNow.AddDays(5),
            null);

        _communityRepositoryMock
            .Setup(r => r.GetByIdAsync(request.CommunityId))
            .ReturnsAsync((Community?)null);

        // Act
        var result = await _meetingService.CreateMeetingAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateMeetingAsync_ShouldCreateMeetingAndGenerateVoterAccesses()
    {
        // Arrange
        var communityId = Guid.NewGuid();
        var community = Community.Create("Comunidad 1", "Direccion 1", "CIF123", Guid.NewGuid());
        var request = new CreateMeetingRequest(
            communityId,
            "Junta Anual",
            MeetingType.Ordinary,
            "Sala de Reuniones",
            DateTime.UtcNow.AddDays(15),
            null);

        _communityRepositoryMock
            .Setup(r => r.GetByIdAsync(communityId))
            .ReturnsAsync(community);

        // Act
        var result = await _meetingService.CreateMeetingAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(request.Title);
        _meetingRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Meeting>()), Times.Once);
        _meetingAccessServiceMock.Verify(m => m.CreateAccessForEligibleMembersAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }
}
