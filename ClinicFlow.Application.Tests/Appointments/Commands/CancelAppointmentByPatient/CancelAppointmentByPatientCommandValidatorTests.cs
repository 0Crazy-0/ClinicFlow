using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByPatient;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.CancelAppointmentByPatient;

public class CancelAppointmentByPatientCommandValidatorTests
{
    private readonly CancelAppointmentByPatientCommandValidator _sut;

    public CancelAppointmentByPatientCommandValidatorTests()
    {
        _sut = new CancelAppointmentByPatientCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
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
