using ClinicFlow.Application.Appointments.Commands.CheckInAppointmentByStaff;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.CheckInAppointmentByStaff;

public class CheckInAppointmentByStaffCommandValidatorTests
{
    private readonly CheckInAppointmentByStaffCommandValidator _sut;

    public CheckInAppointmentByStaffCommandValidatorTests()
    {
        _sut = new CheckInAppointmentByStaffCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CheckInAppointmentByStaffCommand(Guid.NewGuid());

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentIdIsEmpty()
    {
        // Arrange
        var command = new CheckInAppointmentByStaffCommand(Guid.Empty);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AppointmentId);
    }
}
