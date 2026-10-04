using ClinicFlow.Application.Appointments.Commands.UpdateStaffInternalNotesByStaff;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.UpdateStaffInternalNotesByStaff;

public class UpdateStaffInternalNotesByStaffCommandValidatorTests
{
    private readonly UpdateStaffInternalNotesByStaffCommandValidator _sut;

    public UpdateStaffInternalNotesByStaffCommandValidatorTests()
    {
        _sut = new UpdateStaffInternalNotesByStaffCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenRequestIsValid()
    {
        // Arrange
        var command = new UpdateStaffInternalNotesByStaffCommand(
            Guid.CreateVersion7(),
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
        var command = new UpdateStaffInternalNotesByStaffCommand(Guid.Empty, "Valid notes");

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
        var command = new UpdateStaffInternalNotesByStaffCommand(
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
