using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CommunityVoting.UnitTests.Domain;

public class ProposalTests
{
    [Fact]
    public void Create_ShouldInstantiateProposal_WithValidParameters()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var agendaItemId = Guid.NewGuid();
        var title = "Aprobación de Cuentas Anuales";
        var description = "Se somete a votación la aprobación del balance del ejercicio anterior.";
        var order = 1;
        var majorityType = MajorityType.SimpleMajority;
        decimal? majorityPercentage = 50.0m;

        // Act
        var proposal = Proposal.Create(meetingId, agendaItemId, title, description, order, majorityType, majorityPercentage);

        // Assert
        proposal.Should().NotBeNull();
        proposal.Id.Should().NotBeEmpty();
        proposal.MeetingId.Should().Be(meetingId);
        proposal.AgendaItemId.Should().Be(agendaItemId);
        proposal.Title.Should().Be(title);
        proposal.Description.Should().Be(description);
        proposal.Order.Should().Be(order);
        proposal.MajorityType.Should().Be(majorityType);
        proposal.MajorityPercentage.Should().Be(majorityPercentage);
        proposal.Options.Should().BeEmpty();
        proposal.Documents.Should().BeEmpty();
    }
}
