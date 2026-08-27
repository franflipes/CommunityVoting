using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CommunityVoting.UnitTests.Domain;

public class MeetingTests
{
    [Fact]
    public void Create_ShouldInstantiateMeeting_WithCorrectDefaults()
    {
        // Arrange
        var communityId = Guid.NewGuid();
        var title = "Junta General Ordinaria 2026";
        var type = MeetingType.Ordinary;
        var location = "Sala de Juntas";
        var scheduledAt = DateTime.UtcNow.AddDays(10);

        // Act
        var meeting = Meeting.Create(communityId, title, type, location, scheduledAt);

        // Assert
        meeting.Should().NotBeNull();
        meeting.Id.Should().NotBeEmpty();
        meeting.CommunityId.Should().Be(communityId);
        meeting.Title.Should().Be(title);
        meeting.Type.Should().Be(type);
        meeting.Location.Should().Be(location);
        meeting.ScheduledAt.Should().Be(scheduledAt);
        meeting.VotingSettings.Should().NotBeNull();
        meeting.VotingSettingsId.Should().Be(meeting.VotingSettings.Id);
        meeting.AgendaItems.Should().BeEmpty();
        meeting.Proposals.Should().BeEmpty();
        meeting.Participants.Should().BeEmpty();
        meeting.VoterAccesses.Should().BeEmpty();
    }
}
