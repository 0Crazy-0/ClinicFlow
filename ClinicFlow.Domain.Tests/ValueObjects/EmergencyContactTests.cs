using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class EmergencyContactTests
{
    [Fact]
    public void Create_ShouldBuildContactFromStrings()
    {
        // Arrange & Act
        var contact = EmergencyContact.Create("Rainbow", "Dash", "Test", "+1234567890");

        // Assert
        contact.Name.FirstName.Should().Be("Rainbow");
        contact.Name.MiddleName.Should().Be("Dash");
        contact.Name.FirstSurname.Should().Be("Test");
        contact.Phone.Value.Should().Be("+1234567890");
    }

    [Theory]
    [InlineData(null, null, "Doe")]
    [InlineData("Jane", null, null)]
    public void Create_ShouldThrowValueRequired_WhenRequiredPartIsNull(
        string? firstName,
        string? middleName,
        string? firstSurname
    )
    {
        // Arrange & Act
        var act = () =>
            EmergencyContact.Create(firstName!, middleName, firstSurname!, "+1234567890");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData("", "María", "Doe")]
    [InlineData("Jane", null, "")]
    public void Create_ShouldThrowValueRequired_WhenRequiredPartIsBlank(
        string firstName,
        string? middleName,
        string firstSurname
    )
    {
        // Arrange & Act
        var act = () => EmergencyContact.Create(firstName, middleName, firstSurname, "+1234567890");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowValueRequired_WhenMiddleNameProvidedBlank()
    {
        // Arrange & Act
        var act = () => EmergencyContact.Create("Jane", "   ", "Doe", "+1234567890");

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowValueRequired_WhenPhoneIsEmpty(string phone)
    {
        // Arrange & Act
        var act = () => EmergencyContact.Create("Jane", null, "Doe", phone);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowInvalidPhoneFormat_WhenPhoneFormatIsInvalid()
    {
        // Arrange & Act
        var act = () => EmergencyContact.Create("Jane", null, "Doe", "567890");

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidPhoneFormat);
    }

    [Fact]
    public void Create_ShouldThrowRequiredFieldNull_WhenNameIsNull()
    {
        // Arrange & Act
        var act = () => EmergencyContact.Create(null!, PhoneNumber.Create("+1234567890"));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Create_ShouldThrowRequiredFieldNull_WhenPhoneIsNull()
    {
        // Arrange & Act
        var act = () => EmergencyContact.Create(PersonName.Create("Jane", null, "Doe"), null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange & Act & Assert
        EmergencyContact
            .Create("Jane", null, "Doe", "+1234567890")
            .ToString()
            .Should()
            .Be("Jane Doe (+1234567890)");
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var c1 = EmergencyContact.Create("Jane", null, "Doe", "+1234567890");
        var c2 = EmergencyContact.Create("Jane", null, "Doe", "+1234567890");
        var upperCased = EmergencyContact.Create("JANE", null, "DOE", "+1234567890");
        var c3 = EmergencyContact.Create("John", null, "Doe", "+0987654321");

        // Assert
        (c1 == c2)
            .Should()
            .BeTrue();

        (c1 != upperCased).Should().BeTrue();

        (c1 != c3).Should().BeTrue();
    }
}
