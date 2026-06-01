using ClinicFlow.Application.Appointments.Commands.UpdateReceptionistNotesByStaff;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.UpdateReceptionistNotesByStaff;

public class UpdateReceptionistNotesByStaffCommandValidatorTests
{
    private readonly UpdateReceptionistNotesByStaffCommandValidator _sut;

    public UpdateReceptionistNotesByStaffCommandValidatorTests()
    {
        _sut = new UpdateReceptionistNotesByStaffCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenRequestIsValid()
    {
        // Arrange
        var command = new UpdateReceptionistNotesByStaffCommand(Guid.NewGuid(), "Valid notes");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new UpdateReceptionistNotesByStaffCommand(Guid.Empty, "Valid notes");

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
        var command = new UpdateReceptionistNotesByStaffCommand(
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
