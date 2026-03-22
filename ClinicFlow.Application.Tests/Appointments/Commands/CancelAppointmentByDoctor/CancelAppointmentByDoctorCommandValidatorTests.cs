using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByDoctor;
using FluentValidation.TestHelper;
using Xunit;

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
        var command = new CancelAppointmentByDoctorCommand(Guid.NewGuid(), Guid.NewGuid(), "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentByDoctorCommand(Guid.Empty, Guid.NewGuid(), "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentByDoctorCommand(Guid.NewGuid(), Guid.Empty, "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitiatorUserId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenReasonExceedsMaximumLength()
    {
        // Arrange
        var longReason = new string('a', 501);
        var command = new CancelAppointmentByDoctorCommand(Guid.NewGuid(), Guid.NewGuid(), longReason);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
