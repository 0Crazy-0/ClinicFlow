using ClinicFlow.Application.AppointmentTypes.Commands.CreateAppointmentType;
using ClinicFlow.Domain.Enums;
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
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine consultation",
            TimeSpan.FromMinutes(30),
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
    public void Validate_ShouldBeValid_WhenAgeFieldsAreNull()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Checkup,
            "General Checkup",
            "Routine consultation",
            TimeSpan.FromMinutes(30),
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenNameIsEmpty(string? name)
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Checkup,
            name!,
            "Description",
            TimeSpan.FromMinutes(30),
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_ShouldHaveError_WhenDurationIsZeroOrNegative(int minutes)
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            TimeSpan.FromMinutes(minutes),
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationMinutes);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMinimumAgeIsNegative()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            TimeSpan.FromMinutes(30),
            -1,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinimumAge);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMaximumAgeIsNegative()
    {
        // Arrange
        var command = new CreateAppointmentTypeCommand(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            TimeSpan.FromMinutes(30),
            null,
            -5,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaximumAge);
    }
}
