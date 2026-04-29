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
            Guid.NewGuid(),
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
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        // Arrange
        var command = new UpdateAppointmentTypeCommand(
            Guid.Empty,
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            TimeSpan.FromMinutes(30),
            null,
            null,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentTypeId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
