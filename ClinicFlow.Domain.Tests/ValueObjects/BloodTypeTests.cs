using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class BloodTypeTests
{
    // Create
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenValueIsEmpty(string? value)
    {
        // Arrange & Act
        var act = () => BloodType.Create(value!);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Blood type cannot be empty.");
    }

    [Theory]
    [InlineData("X+")]
    [InlineData("C-")]
    [InlineData("ABC")]
    [InlineData("A")]
    public void Create_ShouldThrowException_WhenBloodTypeIsInvalid(string value)
    {
        // Arrange & Act
        var act = () => BloodType.Create(value);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Invalid blood type:*");
    }

    [Theory]
    [InlineData("A+")]
    [InlineData("A-")]
    [InlineData("B+")]
    [InlineData("B-")]
    [InlineData("AB+")]
    [InlineData("AB-")]
    [InlineData("O+")]
    [InlineData("O-")]
    public void Create_ShouldSucceed_ForAllValidBloodTypes(string value)
    {
        // Arrange & Act
        var bloodType = BloodType.Create(value);

        // Assert
        bloodType.Value.Should().Be(value.Trim().ToUpperInvariant());
    }

    [Fact]
    public void Create_ShouldNormalize_ToUpperInvariant()
    {
        // Arrange & Act
        var bloodType = BloodType.Create("ab+");

        // Assert
        bloodType.Value.Should().Be("AB+");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var bloodType = BloodType.Create("  B+  ");

        // Assert
        bloodType.Value.Should().Be("B+");
    }

    // ToString
    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var bloodType = BloodType.Create("O+");

        // Act & Assert
        bloodType.ToString().Should().Be(bloodType.Value);
    }

    // Equality
    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var bt1 = BloodType.Create("A+");
        var bt2 = BloodType.Create("A+");
        var bt3 = BloodType.Create("B-");

        // Act & Assert
        (bt1 == bt2).Should().BeTrue();
        (bt1 != bt3).Should().BeTrue();
    }
}
