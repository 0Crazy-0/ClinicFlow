using ClinicFlow.Application.AppointmentTypes.Commands.CreateAppointmentType;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.CreateAppointmentType;

public class CreateAppointmentTypeCommandValidatorTests
{
    private readonly CreateAppointmentTypeCommandValidator _sut;

    public CreateAppointmentTypeCommandValidatorTests()
    {
        _sut = new CreateAppointmentTypeCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "General Checkup",
            "Routine consultation",
            30,
            18,
            65,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCategoryIsInvalid()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            (AppointmentCategory)999,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            30,
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPurposeIsInvalid()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Other,
            (AppointmentPurpose)999,
            "Checkup",
            "Description",
            30,
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Purpose)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            name!,
            "Routine consultation",
            30,
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(8)]
    [InlineData(95)]
    [InlineData(12)]
    public void Validate_ShouldHaveError_WhenDurationIsInvalid(int minutes)
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            minutes,
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DurationMinutes)
            .WithErrorMessage(DomainErrors.MedicalSpecialty.InvalidEncounterDuration);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenMinimumAgeIsNegative(int minimumAge)
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            30,
            minimumAge,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MinimumAge)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenMaximumAgeIsNegative(int maximumAge)
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            30,
            null,
            maximumAge,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaximumAge)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMinimumAgeExceedsMaximumAllowedAge()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            30,
            AgeEligibilityPolicy.MaximumAllowedAge + 1,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MinimumAge)
            .WithErrorMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMaximumAgeExceedsMaximumAllowedAge()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            30,
            null,
            AgeEligibilityPolicy.MaximumAllowedAge + 1,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaximumAge)
            .WithErrorMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAgeFieldsAreNull()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "General Checkup",
            "Routine consultation",
            30,
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
