using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class MedicalLicenseNumberTests
{
    // Create
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenValueIsEmpty(string? value)
    {
        // Arrange & Act
        var act = () => MedicalLicenseNumber.Create(value!);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("ABC")]
    public void Create_ShouldThrowException_WhenValueIsTooShort(string value)
    {
        // Arrange & Act
        var act = () => MedicalLicenseNumber.Create(value);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var license = MedicalLicenseNumber.Create("  MED-12345  ");

        // Assert
        license.Value.Should().Be("MED-12345");
    }

    // Create - Success
    [Theory]
    [InlineData("ABCD")]
    [InlineData("MED-12345")]
    [InlineData("LIC-2025-001")]
    public void Create_ShouldSucceed_WithValidLicenseNumber(string value)
    {
        // Arrange & Act
        var license = MedicalLicenseNumber.Create(value);

        // Assert
        license.Value.Should().Be(value.Trim());
    }

    // ToString
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var license = MedicalLicenseNumber.Create("MED-12345");

        // Act & Assert
        license.ToString().Should().Be(license.Value);
    }

    // Equality
    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var l1 = MedicalLicenseNumber.Create("MED-12345");
        var l2 = MedicalLicenseNumber.Create("MED-12345");
        var l3 = MedicalLicenseNumber.Create("MED-99999");

        // Act & Assert
        (l1 == l2).Should().BeTrue();
        (l1 != l3).Should().BeTrue();
    }
}
