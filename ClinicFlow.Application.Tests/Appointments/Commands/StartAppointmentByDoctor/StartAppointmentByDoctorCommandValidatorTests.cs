using ClinicFlow.Application.Appointments.Commands.StartAppointmentByDoctor;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.StartAppointmentByDoctor;

public class StartAppointmentByDoctorCommandValidatorTests
{
    private readonly StartAppointmentByDoctorCommandValidator _sut;

    public StartAppointmentByDoctorCommandValidatorTests()
    {
        _sut = new StartAppointmentByDoctorCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new StartAppointmentByDoctorCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new StartAppointmentByDoctorCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new StartAppointmentByDoctorCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitiatorUserId);
    }
}
