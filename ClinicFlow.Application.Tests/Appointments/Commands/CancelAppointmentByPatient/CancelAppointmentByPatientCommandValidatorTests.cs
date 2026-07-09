using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByPatient;
using ClinicFlow.Domain.Common;
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
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
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
        var command = new CancelAppointmentByPatientCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            "Reason"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentByPatientCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            "Reason"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenReasonExceedsMaximumLength()
    {
        // Arrange
        var longReason = new string('a', 501);
        var command = new CancelAppointmentByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            longReason
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
