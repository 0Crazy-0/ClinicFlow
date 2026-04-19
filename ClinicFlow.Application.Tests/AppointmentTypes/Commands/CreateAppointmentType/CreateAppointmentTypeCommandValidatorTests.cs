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
}
