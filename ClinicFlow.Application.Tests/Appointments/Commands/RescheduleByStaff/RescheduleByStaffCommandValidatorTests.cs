using ClinicFlow.Application.Appointments.Commands.RescheduleByStaff;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.RescheduleByStaff;

public class RescheduleByStaffCommandValidatorTests
{
    private readonly RescheduleByStaffCommandValidator _sut;

    public RescheduleByStaffCommandValidatorTests()
    {
        _sut = new RescheduleByStaffCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new RescheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
