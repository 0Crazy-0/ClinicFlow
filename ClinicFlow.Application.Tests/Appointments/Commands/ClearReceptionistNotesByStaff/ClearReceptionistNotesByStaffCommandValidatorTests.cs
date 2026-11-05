using ClinicFlow.Application.Appointments.Commands.ClearReceptionistNotesByStaff;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ClearReceptionistNotesByStaff;

public class ClearReceptionistNotesByStaffCommandValidatorTests
{
    private readonly ClearReceptionistNotesByStaffCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldBeValid_WhenRequestIsValid()
    {
        // Arrange
        var command = new ClearReceptionistNotesByStaffCommand(Guid.CreateVersion7());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new ClearReceptionistNotesByStaffCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AppointmentId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
