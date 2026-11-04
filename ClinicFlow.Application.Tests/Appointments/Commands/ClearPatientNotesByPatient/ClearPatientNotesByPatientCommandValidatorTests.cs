using ClinicFlow.Application.Appointments.Commands.ClearPatientNotesByPatient;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ClearPatientNotesByPatient;

public class ClearPatientNotesByPatientCommandValidatorTests
{
    private readonly ClearPatientNotesByPatientCommandValidator _sut;

    public ClearPatientNotesByPatientCommandValidatorTests()
    {
        _sut = new ClearPatientNotesByPatientCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenRequestIsValid()
    {
        // Arrange
        var command = new ClearPatientNotesByPatientCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7()
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
        var command = new ClearPatientNotesByPatientCommand(Guid.Empty, Guid.CreateVersion7());

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
        var command = new ClearPatientNotesByPatientCommand(Guid.CreateVersion7(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.InitiatorUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
