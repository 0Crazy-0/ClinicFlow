using ClinicFlow.Application.Appointments.Commands.ScheduleByDoctor;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleByDoctor;

public class ScheduleByDoctorCommandValidatorTests
{
    private readonly ScheduleByDoctorCommandValidator _sut;

    public ScheduleByDoctorCommandValidatorTests()
    {
        _sut = new ScheduleByDoctorCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new ScheduleByDoctorCommand(
            Guid.NewGuid(),
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
