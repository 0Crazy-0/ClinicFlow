using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class PersonNameTests
{
    // Create
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenValueIsEmpty(string? value)
    {
        // Arrange & Act
        var act = () => PersonName.Create(value!);

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Name cannot be empty.");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenNameIsTooShort()
    {
        // Arrange & Act
        var act = () => PersonName.Create("A");

        // Assert
        act.Should().Throw<BusinessRuleValidationException>().WithMessage("Name is too short.");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var name = PersonName.Create("  John Doe  ");

        // Assert
        name.FullName.Should().Be("John Doe");
    }

    [Theory]
    [InlineData("Jo")]
    [InlineData("John Doe")]
    [InlineData("María García López")]
    public void Create_ShouldSucceed_WithValidName(string value)
    {
        // Arrange & Act
        var name = PersonName.Create(value);

        // Assert
        name.FullName.Should().Be(value.Trim());
    }

    // ToString
    [Fact]
    public void ToString_ShouldReturnFullName()
    {
        // Arrange
        var name = PersonName.Create("John Doe");

        // Act & Assert
        name.ToString().Should().Be(name.FullName);
    }

    // Equality
    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var n1 = PersonName.Create("John Doe");
        var n2 = PersonName.Create("John Doe");
        var n3 = PersonName.Create("Jane Doe");

        // Act & Assert
        (n1 == n2).Should().BeTrue();
        (n1 != n3).Should().BeTrue();
    }
}
