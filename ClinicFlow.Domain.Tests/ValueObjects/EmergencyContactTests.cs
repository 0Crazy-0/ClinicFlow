using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class EmergencyContactTests
{
    // Create
    [Fact]
    public void Create_WithStrings_ShouldSucceed()
    {
        // Arrange & Act
        var contact = EmergencyContact.Create("Jane Doe", "+1234567890");

        // Assert
        contact.Name.FullName.Should().Be("Jane Doe");
        contact.PhoneNumber.Value.Should().Be("+1234567890");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithStrings_ShouldThrowException_WhenNameIsInvalid(string? name)
    {
        // Arrange & Act
        var act = () => EmergencyContact.Create(name!, "+1234567890");

        // Assert
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithStrings_ShouldThrowException_WhenPhoneIsInvalid(string? phone)
    {
        // Arrange & Act
        var act = () => EmergencyContact.Create("Jane Doe", phone!);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>();
    }

    [Fact]
    public void Create_WithValueObjects_ShouldThrowException_WhenNameIsNull()
    {
        //Arrange & Act
        var act = () => EmergencyContact.Create(null!, PhoneNumber.Create("+1234567890"));

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Create_WithValueObjects_ShouldThrowException_WhenPhoneIsNull()
    {
        // Arrange & Act
        var act = () => EmergencyContact.Create(PersonName.Create("Jane Doe"), null!);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    // ToString
    [Fact]
    public void ToString_ShouldReturnFormattedString() => EmergencyContact.Create("Jane Doe", "+1234567890").ToString().Should().Be("Jane Doe (+1234567890)"); // Arrange & Act & Assert

    // Equality
    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var c1 = EmergencyContact.Create("Jane Doe", "+1234567890");
        var c2 = EmergencyContact.Create("Jane Doe", "+1234567890");
        var c3 = EmergencyContact.Create("John Doe", "+0987654321");

        // Act & Assert
        (c1 == c2).Should().BeTrue();
        (c1 != c3).Should().BeTrue();
    }
}
