using CommunityVoting.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace CommunityVoting.UnitTests.Domain;

public class CommunityTests
{
    [Fact]
    public void Create_ShouldInstantiateCommunity_AndInitializeVotingSettings()
    {
        // Arrange
        var name = "Residencial Los Olivos";
        var address = "Calle Mayor 12, Madrid";
        var cif = "H12345678";
        var createdByUserId = Guid.NewGuid();

        // Act
        var community = Community.Create(name, address, cif, createdByUserId);

        // Assert
        community.Should().NotBeNull();
        community.Id.Should().NotBeEmpty();
        community.Name.Should().Be(name);
        community.Address.Should().Be(address);
        community.Cif.Should().Be(cif);
        community.CreatedByUserId.Should().Be(createdByUserId);
        community.VotingSettings.Should().NotBeNull();
        community.VotingSettingsId.Should().Be(community.VotingSettings.Id);
        community.Members.Should().BeEmpty();
        community.Meetings.Should().BeEmpty();
        community.Invitations.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldAllowNullCif()
    {
        // Act
        var community = Community.Create("Comunidad Sin Cif", "Av. Libertad 5", null, Guid.NewGuid());

        // Assert
        community.Cif.Should().BeNull();
    }
}
