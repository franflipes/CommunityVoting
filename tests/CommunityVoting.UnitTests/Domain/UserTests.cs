using CommunityVoting.Domain.Entities;
using CommunityVoting.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace CommunityVoting.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_ShouldInstantiateUser_WithValidParameters()
    {
        // Arrange
        var name = "Carlos";
        var lastName = "Gomez";
        var email = "carlos.gomez@example.com";
        var phone = "+34600000000";
        var passwordHash = "hashed_password_123";
        var role = UserRole.CommunityAdmin;

        // Act
        var user = User.Create(name, lastName, email, phone, passwordHash, role);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be(name);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
        user.PhoneNumber.Should().Be(phone);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(role);
        user.Memberships.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Create_ShouldSetDefaultRoleToCommunityMember_WhenSpecified()
    {
        // Act
        var user = User.Create("Anna", "Lopez", "anna@example.com", "123456", "hash", UserRole.CommunityMember);

        // Assert
        user.Role.Should().Be(UserRole.CommunityMember);
    }
}
