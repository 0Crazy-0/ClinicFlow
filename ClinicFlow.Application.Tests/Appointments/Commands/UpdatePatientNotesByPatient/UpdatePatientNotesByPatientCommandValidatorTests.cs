using ClinicFlow.Application.Appointments.Commands.UpdatePatientNotesByPatient;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.UpdatePatientNotesByPatient;

public class UpdatePatientNotesByPatientCommandValidatorTests
{
    private readonly UpdatePatientNotesByPatientCommandValidator _sut;

    public UpdatePatientNotesByPatientCommandValidatorTests()
    {
        _sut = new UpdatePatientNotesByPatientCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenRequestIsValid()
    {
        // Arrange
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Valid notes"
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
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.Empty,
            Guid.NewGuid(),
            "Valid notes"
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
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.NewGuid(),
            Guid.Empty,
            "Valid notes"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNotesTooLong()
    {
        // Arrange
        var command = new UpdatePatientNotesByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new string('a', 501)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
