using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommunityVoting.UnitTests.Application;

public class ProposalServiceTests
{
    private readonly Mock<IProposalRepository> _proposalRepositoryMock;
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly ProposalService _proposalService;

    public ProposalServiceTests()
    {
        _proposalRepositoryMock = new Mock<IProposalRepository>();
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _proposalService = new ProposalService(_proposalRepositoryMock.Object, _meetingRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateProposalAsync_ShouldReturnNull_WhenMeetingDoesNotExist()
    {
        // Arrange
        var request = new CreateProposalRequest(
            Guid.NewGuid(), Guid.NewGuid(), "Votacion Reforma", "Descripcion", 1, MajorityType.SimpleMajority, 50.0m, new List<string> { "A Favor", "En Contra" });

        _meetingRepositoryMock
            .Setup(m => m.GetByIdAsync(request.MeetingId))
            .ReturnsAsync((Meeting?)null);

        // Act
        var result = await _proposalService.CreateProposalAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateProposalAsync_ShouldCreateProposalWithOptions_WhenMeetingExists()
    {
        // Arrange
        var meeting = Meeting.Create(Guid.NewGuid(), "Junta 2026", MeetingType.Ordinary, "Loc", DateTime.UtcNow);
        var request = new CreateProposalRequest(
            meeting.Id, Guid.NewGuid(), "Aprobación Balance", "Balance de cuentas", 1, MajorityType.SimpleMajority, 50.0m, new List<string> { "Sí", "No", "Abstención" });

        _meetingRepositoryMock
            .Setup(m => m.GetByIdAsync(meeting.Id))
            .ReturnsAsync(meeting);

        // Act
        var result = await _proposalService.CreateProposalAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(request.Title);
        result.Options.Should().HaveCount(3);
        _proposalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Proposal>()), Times.Once);
    }
}
