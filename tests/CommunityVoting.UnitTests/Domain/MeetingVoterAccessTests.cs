using CommunityVoting.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CommunityVoting.UnitTests.Domain;

public class MeetingVoterAccessTests
{
    [Fact]
    public void Create_ShouldInstantiateVoterAccess_WithHashedCredentials()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tokenHash = "token_hash_abc";
        var codeHash = "code_hash_123";
        var expiresAt = DateTimeOffset.UtcNow.AddDays(1);

        // Act
        var access = MeetingVoterAccess.Create(meetingId, userId, tokenHash, codeHash, expiresAt);

        // Assert
        access.Should().NotBeNull();
        access.Id.Should().NotBeEmpty();
        access.MeetingId.Should().Be(meetingId);
        access.UserId.Should().Be(userId);
        access.TokenHash.Should().Be(tokenHash);
        access.CodeHash.Should().Be(codeHash);
        access.ExpiresAt.Should().Be(expiresAt);
        access.IsRevoked.Should().BeFalse();
        access.FailedAttempts.Should().Be(0);
        access.LastUsedAt.Should().BeNull();
    }
}
