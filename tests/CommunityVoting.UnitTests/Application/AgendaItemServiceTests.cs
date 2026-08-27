using CommunityVoting.Application.DTOs;
using CommunityVoting.Application.Interfaces;
using CommunityVoting.Application.Services;
using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace CommunityVoting.UnitTests.Application;

public class AgendaItemServiceTests
{
    private readonly Mock<IAgendaItemRepository> _agendaItemRepositoryMock;
    private readonly Mock<IMeetingRepository> _meetingRepositoryMock;
    private readonly AgendaItemService _agendaItemService;

    public AgendaItemServiceTests()
    {
        _agendaItemRepositoryMock = new Mock<IAgendaItemRepository>();
        _meetingRepositoryMock = new Mock<IMeetingRepository>();
        _agendaItemService = new AgendaItemService(_agendaItemRepositoryMock.Object, _meetingRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnNull_WhenMeetingDoesNotExist()
    {
        // Arrange
        var request = new CreateAgendaItemRequest(Guid.NewGuid(), "Lectura del Acta", "Punto 1", 1);
        _meetingRepositoryMock
            .Setup(m => m.GetByIdAsync(request.MeetingId))
            .ReturnsAsync((Meeting?)null);

        // Act
        var result = await _agendaItemService.CreateAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnItem_WhenMeetingExists()
    {
        // Arrange
        var meeting = Meeting.Create(Guid.NewGuid(), "Junta 2026", MeetingType.Ordinary, "Loc", DateTime.UtcNow);
        var request = new CreateAgendaItemRequest(meeting.Id, "Punto 1: Presupuestos", "Descripcion", 1);

        _meetingRepositoryMock
            .Setup(m => m.GetByIdAsync(meeting.Id))
            .ReturnsAsync(meeting);

        _agendaItemRepositoryMock
            .Setup(a => a.GetByMeetingIdAsync(meeting.Id))
            .ReturnsAsync(new List<AgendaItem>());

        _agendaItemRepositoryMock
            .Setup(a => a.AddAsync(It.IsAny<AgendaItem>()))
            .ReturnsAsync((AgendaItem item) => item);

        // Act
        var result = await _agendaItemService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be(request.Title);
        result.Order.Should().Be(request.Order);
        _agendaItemRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AgendaItem>()), Times.Once);
    }
}
