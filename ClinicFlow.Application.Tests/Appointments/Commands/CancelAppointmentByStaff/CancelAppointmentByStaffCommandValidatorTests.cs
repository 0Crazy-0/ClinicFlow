using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

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
        var command = new CancelAppointmentByStaffCommand(
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
        var command = new CancelAppointmentByStaffCommand(
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
        var command = new CancelAppointmentByStaffCommand(
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenReasonIsEmpty(string? reason)
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            reason!
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenReasonExceedsMaximumLength()
    {
        // Arrange
        var longReason = new string('a', 501);
        var command = new CancelAppointmentByStaffCommand(
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
