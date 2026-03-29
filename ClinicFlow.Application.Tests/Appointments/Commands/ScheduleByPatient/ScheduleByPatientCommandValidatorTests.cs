using ClinicFlow.Application.Appointments.Commands.ScheduleByPatient;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.ScheduleByPatient;

public class ScheduleByPatientCommandValidatorTests
{
    private readonly ScheduleByPatientCommandValidator _sut;

    public ScheduleByPatientCommandValidatorTests()
    {
        _sut = new ScheduleByPatientCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        var command = new ScheduleByPatientCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var result = _sut.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
