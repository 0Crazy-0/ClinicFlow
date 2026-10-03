using ClinicFlow.Application.Appointments.Commands.UpdateGuardianNotesByGuardian;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.UpdateGuardianNotesByGuardian;

public class UpdateGuardianNotesByGuardianCommandValidatorTests
{
    private readonly UpdateGuardianNotesByGuardianCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldNotHaveError_WhenCommandIsValid()
    {
        // Arrange
        var command = new UpdateGuardianNotesByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "Notes"
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
        var command = new UpdateGuardianNotesByGuardianCommand(
            Guid.Empty,
            Guid.CreateVersion7(),
            "Notes"
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
        var command = new UpdateGuardianNotesByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.Empty,
            "Notes"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNotesAreTooLong()
    {
        // Arrange
        var command = new UpdateGuardianNotesByGuardianCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
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
