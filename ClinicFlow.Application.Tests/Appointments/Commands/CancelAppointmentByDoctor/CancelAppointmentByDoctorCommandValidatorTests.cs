using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByDoctor;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.CancelAppointmentByDoctor;

public class CancelAppointmentByDoctorCommandValidatorTests
{
    private readonly CancelAppointmentByDoctorCommandValidator _sut;

    public CancelAppointmentByDoctorCommandValidatorTests()
    {
        _sut = new CancelAppointmentByDoctorCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CancelAppointmentByDoctorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Reason"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
