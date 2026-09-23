using ClinicFlow.Application.AppointmentTypes.Commands.UpdateAppointmentType;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.AppointmentTypes.Commands.UpdateAppointmentType;

public class UpdateAppointmentTypeCommandValidatorTests
{
    private readonly UpdateAppointmentTypeCommandValidator _sut;

    public UpdateAppointmentTypeCommandValidatorTests()
    {
        _sut = new UpdateAppointmentTypeCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new UpdateAppointmentTypeCommand(
            Guid.CreateVersion7(),
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "General Checkup",
            "Routine consultation",
            30
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new UpdateAppointmentTypeCommand(
            Guid.Empty,
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            30
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenCategoryIsInvalid()
    {
        // Arrange
        var command = new UpdateAppointmentTypeCommand(
            Guid.CreateVersion7(),
            (AppointmentCategory)999,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            30
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
        var command = new UpdateAppointmentTypeCommand(
            Guid.CreateVersion7(),
            AppointmentCategory.Other,
            (AppointmentPurpose)999,
            "Checkup",
            "Description",
            30
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
        var command = new UpdateAppointmentTypeCommand(
            Guid.CreateVersion7(),
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            name!,
            "Description",
            30
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
        var command = new UpdateAppointmentTypeCommand(
            Guid.CreateVersion7(),
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            minutes
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DurationMinutes)
            .WithErrorMessage(DomainErrors.MedicalSpecialty.InvalidEncounterDuration);
    }
}
