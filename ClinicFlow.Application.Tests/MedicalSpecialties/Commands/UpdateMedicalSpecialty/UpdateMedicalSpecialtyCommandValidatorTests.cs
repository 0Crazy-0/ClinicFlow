using ClinicFlow.Application.MedicalSpecialties.Commands.UpdateMedicalSpecialty;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.MedicalSpecialties.Commands.UpdateMedicalSpecialty;

public class UpdateMedicalSpecialtyCommandValidatorTests
{
    private readonly UpdateMedicalSpecialtyCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new UpdateMedicalSpecialtyCommand(
            Guid.NewGuid(),
            "Cardiology",
            "Heart specialty",
            30,
            24
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenSpecialtyIdIsEmpty()
    {
        // Arrange
        var command = new UpdateMedicalSpecialtyCommand(
            Guid.Empty,
            "Cardiology",
            "Heart specialty",
            30,
            24
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SpecialtyId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldFail_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var command = new UpdateMedicalSpecialtyCommand(
            Guid.NewGuid(),
            name!,
            "Heart specialty",
            30,
            24
        );

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
        var command = new UpdateMedicalSpecialtyCommand(
            Guid.NewGuid(),
            "Cardiology",
            description!,
            30,
            24
        );

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
        var command = new UpdateMedicalSpecialtyCommand(
            Guid.NewGuid(),
            "Cardiology",
            "Heart specialty",
            0,
            24
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.TypicalDurationMinutes)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(47)]
    [InlineData(100)]
    public void Validate_ShouldFail_WhenMinCancellationHoursIsInvalid(int hours)
    {
        // Arrange
        var command = new UpdateMedicalSpecialtyCommand(
            Guid.NewGuid(),
            "Cardiology",
            "Heart specialty",
            30,
            hours
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MinCancellationHours)
            .WithErrorMessage(DomainErrors.MedicalSpecialty.InvalidCancellationLimit);
    }
}
