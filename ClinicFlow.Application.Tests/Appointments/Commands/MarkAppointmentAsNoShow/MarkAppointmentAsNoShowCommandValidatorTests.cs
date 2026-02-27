using ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShow;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.MarkAppointmentAsNoShow;

public class MarkAppointmentAsNoShowCommandValidatorTests
{
    private readonly MarkAppointmentAsNoShowCommandValidator _sut;

    public MarkAppointmentAsNoShowCommandValidatorTests()
    {
        _sut = new MarkAppointmentAsNoShowCommandValidator();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId).WithErrorMessage("Appointment ID is required.");
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitiatorUserId).WithErrorMessage("Initiator User ID is required.");
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}