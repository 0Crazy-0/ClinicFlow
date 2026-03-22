using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace ClinicFlow.Application.Tests.Appointments.Commands.CancelAppointmentByStaff;

public class CancelAppointmentByStaffCommandValidatorTests
{
    private readonly CancelAppointmentByStaffCommandValidator _sut;

    public CancelAppointmentByStaffCommandValidatorTests()
    {
        _sut = new CancelAppointmentByStaffCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(Guid.NewGuid(), Guid.NewGuid(), UserRole.Admin, "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(Guid.Empty, Guid.NewGuid(), UserRole.Admin, "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(Guid.NewGuid(), Guid.Empty, UserRole.Admin, "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitiatorUserId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenReasonIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(Guid.NewGuid(), Guid.NewGuid(), UserRole.Admin, string.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenReasonExceedsMaximumLength()
    {
        // Arrange
        var longReason = new string('a', 501);
        var command = new CancelAppointmentByStaffCommand(Guid.NewGuid(), Guid.NewGuid(), UserRole.Admin, longReason);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
