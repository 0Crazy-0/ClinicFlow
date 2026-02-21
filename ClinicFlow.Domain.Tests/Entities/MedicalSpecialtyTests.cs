using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class MedicalSpecialtyTests
{
    // Create
    [Fact]
    public void Create_ShouldCreateSpecialty_WhenValidParameters()
    {
        // Arrange
        var name = "Cardiology";
        var description = "Heart specialty";
        var typicalDuration = 30;
        var minCancellationHours = 24;

        // Act
        var specialty = MedicalSpecialty.Create(name, description, typicalDuration, minCancellationHours);

        // Assert
        specialty.Should().NotBeNull();
        specialty.Name.Should().Be(name);
        specialty.Description.Should().Be(description);
        specialty.TypicalDurationMinutes.Should().Be(typicalDuration);
        specialty.MinCancellationHours.Should().Be(minCancellationHours);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenNameIsEmpty(string? name)
    {
        // Arrange & Act
        var act = () => MedicalSpecialty.Create(name!, "Description", 30, 24);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Specialty name cannot be empty.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_ShouldThrowException_WhenDurationIsZeroOrNegative(int duration)
    {
        // Arrange & Act
        var act = () => MedicalSpecialty.Create("Cardiology", "Description", duration, 24);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Duration must be positive.");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenMinCancellationHoursIsNegative()
    {
        // Arrange & Act
        var act = () => MedicalSpecialty.Create("Cardiology", "Description", 30, -1);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Cancellation hours cannot be negative.");
    }

    // IsCancellationAllowed
    [Fact]
    public void IsCancellationAllowed_ShouldReturnTrue_WhenSufficientNotice() => MedicalSpecialty.Create("Cardiology", "Description", 30, 24)
        .IsCancellationAllowed(DateTime.UtcNow.AddHours(48)).Should().BeTrue(); // Arrange & Act & Assert

    [Fact]
    public void IsCancellationAllowed_ShouldReturnFalse_WhenInsufficientNotice() => MedicalSpecialty.Create("Cardiology", "Description", 30, 24)
        .IsCancellationAllowed(DateTime.UtcNow.AddHours(2)).Should().BeFalse(); // Arrange & Act & Assert

}  
