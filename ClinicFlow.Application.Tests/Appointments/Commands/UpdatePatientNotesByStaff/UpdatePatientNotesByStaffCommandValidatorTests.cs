using ClinicFlow.Application.Appointments.Commands.UpdatePatientNotesByStaff;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.UpdatePatientNotesByStaff;

public class UpdatePatientNotesByStaffCommandValidatorTests
{
    private readonly UpdatePatientNotesByStaffCommandValidator _sut;

    public UpdatePatientNotesByStaffCommandValidatorTests()
    {
        _sut = new UpdatePatientNotesByStaffCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenRequestIsValid()
    {
        // Arrange
        var command = new UpdatePatientNotesByStaffCommand(Guid.NewGuid(), "Valid notes");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new UpdatePatientNotesByStaffCommand(Guid.Empty, "Valid notes");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenNotesTooLong()
    {
        // Arrange
        var command = new UpdatePatientNotesByStaffCommand(Guid.NewGuid(), new string('a', 501));

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Notes)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
