using ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByStaff;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.MarkAppointmentAsNoShowByStaff;

public class MarkAppointmentAsNoShowByStaffCommandValidatorTests
{
    private readonly MarkAppointmentAsNoShowByStaffCommandValidator _sut;

    public MarkAppointmentAsNoShowByStaffCommandValidatorTests()
    {
        _sut = new MarkAppointmentAsNoShowByStaffCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByStaffCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByStaffCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        // Arrange
        var command = new MarkAppointmentAsNoShowByStaffCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InitiatorUserId);
    }
}
