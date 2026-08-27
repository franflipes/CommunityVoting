using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CommunityVoting.UnitTests.Domain;

public class EmailOutboxMessageTests
{
    [Fact]
    public void Create_ShouldInstantiateOutboxMessage_WithPendingStatus()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var type = "MeetingAccess";
        var recipient = "user@example.com";
        var template = "MeetingAccessTemplate";
        var payload = "{\"token\":\"abc\"}";

        // Act
        var message = EmailOutboxMessage.Create(type, recipient, template, payload, meetingId, userId);

        // Assert
        message.Should().NotBeNull();
        message.Id.Should().NotBeEmpty();
        message.MeetingId.Should().Be(meetingId);
        message.UserId.Should().Be(userId);
        message.Type.Should().Be(type);
        message.Recipient.Should().Be(recipient);
        message.Template.Should().Be(template);
        message.Payload.Should().Be(payload);
        message.Status.Should().Be(EmailStatus.Pending);
        message.Attempts.Should().Be(0);
        message.ProcessedAt.Should().BeNull();
        message.Error.Should().BeNull();
    }
}
