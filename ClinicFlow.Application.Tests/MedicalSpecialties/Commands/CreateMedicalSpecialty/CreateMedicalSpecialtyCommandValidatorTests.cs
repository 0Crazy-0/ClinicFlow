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

    [Fact]
    public void Validate_ShouldFail_WhenTypicalDurationMinutesIsZero()
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand("Cardiology", "Heart specialty", 0, 24);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TypicalDurationMinutes)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Fact]
    public void Validate_ShouldFail_WhenMinCancellationHoursIsNegative()
    {
        // Arrange
        var command = new CreateMedicalSpecialtyCommand("Cardiology", "Heart specialty", 30, -1);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MinCancellationHours)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }
}
