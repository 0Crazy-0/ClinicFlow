using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Schedule;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Services;

public class WeeklyScheduleSetupServiceTests
{
    [Fact]
    public void SetupWeeklySchedule_ShouldCreateAllSchedules_WhenNoDuplicatesExist()
    {
        // Arrange
        var doctorId = Guid.NewGuid();

        var slots = new List<WeeklyScheduleSlot>
        {
            new(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
            new(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
            new(DayOfWeek.Friday, new TimeOnly(14, 0), new TimeOnly(18, 0)),
        };

        // Act
        var result = WeeklyScheduleSetupService.SetupWeeklySchedule(doctorId, [], slots).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(s => s.DoctorId.Should().Be(doctorId));
        result.Should().AllSatisfy(s => s.IsActive.Should().BeTrue());

        result[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        result[0].TimeRange.Start.Should().Be(new TimeOnly(8, 0));
        result[0].TimeRange.End.Should().Be(new TimeOnly(13, 0));

        result[1].DayOfWeek.Should().Be(DayOfWeek.Wednesday);
        result[2].DayOfWeek.Should().Be(DayOfWeek.Friday);
        result[2].TimeRange.Start.Should().Be(new TimeOnly(14, 0));
        result[2].TimeRange.End.Should().Be(new TimeOnly(18, 0));
    }

    [Fact]
    public void SetupWeeklySchedule_ShouldThrowException_WhenDuplicateDayExistsInExistingSchedules()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var existingSchedules = new List<Schedule>
        {
            Schedule.Create(
                doctorId,
                DayOfWeek.Monday,
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
            ),
        };

        var slots = new List<WeeklyScheduleSlot>
        {
            new(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
            new(DayOfWeek.Wednesday, new TimeOnly(8, 0), new TimeOnly(13, 0)),
        };

        // Act
        var act = () =>
            WeeklyScheduleSetupService.SetupWeeklySchedule(doctorId, existingSchedules, slots);

        // Assert
        var exceptionAssertion = act.Should()
            .Throw<ScheduleAlreadyExistsException>()
            .WithMessage(DomainErrors.Schedule.ScheduleAlreadyExists);
        exceptionAssertion.Which.DoctorId.Should().Be(doctorId);
        exceptionAssertion.Which.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void SetupWeeklySchedule_ShouldCreateSchedules_WhenExistingScheduleIsInactive()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var inactiveSchedule = Schedule.Create(
            doctorId,
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );
        inactiveSchedule.Deactivate();

        var existingSchedules = new List<Schedule> { inactiveSchedule };

        var slots = new List<WeeklyScheduleSlot>
        {
            new(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)),
        };

        // Act
        var result = WeeklyScheduleSetupService
            .SetupWeeklySchedule(doctorId, existingSchedules, slots)
            .ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        result[0].IsActive.Should().BeTrue();
    }
}
