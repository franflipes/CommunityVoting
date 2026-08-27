using CommunityVoting.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CommunityVoting.UnitTests.Domain;

public class CommunityInvitationTests
{
    [Fact]
    public void Create_ShouldInstantiateInvitation_WithUniqueToken()
    {
        // Arrange
        var communityId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();
        var expiresInDays = 7;
        var maxUses = 5;

        // Act
        var invitation = CommunityInvitation.Create(communityId, createdByUserId, expiresInDays, maxUses);

        // Assert
        invitation.Should().NotBeNull();
        invitation.Id.Should().NotBeEmpty();
        invitation.CommunityId.Should().Be(communityId);
        invitation.CreatedByUserId.Should().Be(createdByUserId);
        invitation.Token.Should().NotBeNullOrEmpty();
        invitation.MaxUses.Should().Be(maxUses);
        invitation.UsesCount.Should().Be(0);
        invitation.ExpiresAt.Should().NotBeNull();
        invitation.IsActive.Should().BeTrue();
    }
}
