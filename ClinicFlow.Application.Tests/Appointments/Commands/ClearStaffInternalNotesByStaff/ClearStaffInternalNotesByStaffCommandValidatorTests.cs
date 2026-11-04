using ClinicFlow.Application.Appointments.Commands.ClearStaffInternalNotesByStaff;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ClearStaffInternalNotesByStaff;

public class ClearStaffInternalNotesByStaffCommandValidatorTests
{
    private readonly ClearStaffInternalNotesByStaffCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenRequestIsValid()
    {
        // Arrange
        var command = new ClearStaffInternalNotesByStaffCommand(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new ClearStaffInternalNotesByStaffCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}

