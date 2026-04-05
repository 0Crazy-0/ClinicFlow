using ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;
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
        var command = new CancelAppointmentByStaffCommand(Guid.NewGuid(), Guid.NewGuid(), "Reason");

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenReasonIsEmpty()
    {
        // Arrange
        var command = new CancelAppointmentByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            string.Empty
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason);
    }
}
