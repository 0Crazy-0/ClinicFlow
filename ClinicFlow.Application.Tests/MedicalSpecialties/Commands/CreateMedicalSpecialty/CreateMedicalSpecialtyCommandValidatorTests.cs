using ClinicFlow.Application.MedicalSpecialties.Commands.CreateMedicalSpecialty;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Commands.CreateMedicalSpecialty;

public class CreateMedicalSpecialtyCommandValidatorTests
{
    private readonly CreateMedicalSpecialtyCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand("Cardiology", "Heart specialty", 30, 24);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand(name!, "Heart specialty", 30, 24);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenDescriptionIsEmpty(string? description)
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand("Cardiology", description!, 30, 24);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(44)]
    [InlineData(95)]
    public void Validate_ShouldFail_WhenTypicalDurationMinutesIsInvalid(int duration)
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand(
            "Cardiology",
            "Heart specialty",
            duration,
            24
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TypicalDurationMinutes)
            .WithErrorMessage(DomainErrors.MedicalSpecialty.InvalidEncounterDuration);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(23)]
    [InlineData(75)]
    public void Validate_ShouldFail_WhenMinCancellationHoursIsInvalid(int hours)
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand("Cardiology", "Heart specialty", 30, hours);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MinCancellationHours)
            .WithErrorMessage(DomainErrors.MedicalSpecialty.InvalidCancellationLimit);
    }
}
