using ClinicFlow.Application.Appointments.Commands.Shared.Schedule;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Appointments.Commands.Shared.Schedule;

public record DummyScheduleCommand(
    Guid InitiatorUserId,
    Guid TargetPatientId,
    Guid AppointmentTypeId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime
) : IScheduleCommand;

public class DummyScheduleCommandValidator : ScheduleCommandValidatorBase<DummyScheduleCommand> { }

public class ScheduleCommandValidatorBaseTests
{
    private readonly DummyScheduleCommandValidator _sut;

    public ScheduleCommandValidatorBaseTests()
    {
        _sut = new DummyScheduleCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        var command = new DummyScheduleCommand(
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

    [Fact]
    public void Validate_ShouldHaveError_WhenInitiatorUserIdIsEmpty()
    {
        var command = new DummyScheduleCommand(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.InitiatorUserId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenTargetPatientIdIsEmpty()
    {
        var command = new DummyScheduleCommand(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetPatientId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAppointmentTypeIdIsEmpty()
    {
        var command = new DummyScheduleCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AppointmentTypeId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenScheduledDateIsInThePast()
    {
        var command = new DummyScheduleCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-1).Date,
            new TimeSpan(10, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ScheduledDate);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenStartTimeIsAfterEndTime()
    {
        var command = new DummyScheduleCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(1).Date,
            new TimeSpan(12, 0, 0),
            new TimeSpan(11, 0, 0)
        );

        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.StartTime);
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }
}
