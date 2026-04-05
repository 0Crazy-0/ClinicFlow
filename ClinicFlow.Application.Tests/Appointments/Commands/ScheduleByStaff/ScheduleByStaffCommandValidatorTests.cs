using ClinicFlow.Application.Appointments.Commands.ScheduleByStaff;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleByStaff;

public class ScheduleByStaffCommandValidatorTests
{
    private readonly ScheduleByStaffCommandValidator _sut;

    public ScheduleByStaffCommandValidatorTests()
    {
        _sut = new ScheduleByStaffCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new ScheduleByStaffCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0),
            false,
            false
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
