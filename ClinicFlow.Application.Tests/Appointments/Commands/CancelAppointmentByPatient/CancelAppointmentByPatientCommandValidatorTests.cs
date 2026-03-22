using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByPatient;
using FluentValidation.TestHelper;
using Xunit;

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

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(Guid.Empty, Guid.NewGuid(), "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(Guid.NewGuid(), Guid.Empty, "Reason");

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
        var command = new CancelAppointmentByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            longReason
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
