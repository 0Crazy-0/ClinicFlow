using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Schedule;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

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
            new(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(13)),
            new(DayOfWeek.Wednesday, TimeSpan.FromHours(8), TimeSpan.FromHours(13)),
            new(DayOfWeek.Friday, TimeSpan.FromHours(14), TimeSpan.FromHours(18)),
        };

        // Act
        var result = WeeklyScheduleSetupService.SetupWeeklySchedule(doctorId, [], slots);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(s => s.DoctorId.Should().Be(doctorId));
        result.Should().AllSatisfy(s => s.IsActive.Should().BeTrue());

        result[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        result[0].TimeRange.Start.Should().Be(TimeSpan.FromHours(8));
        result[0].TimeRange.End.Should().Be(TimeSpan.FromHours(13));

        result[1].DayOfWeek.Should().Be(DayOfWeek.Wednesday);
        result[2].DayOfWeek.Should().Be(DayOfWeek.Friday);
        result[2].TimeRange.Start.Should().Be(TimeSpan.FromHours(14));
        result[2].TimeRange.End.Should().Be(TimeSpan.FromHours(18));
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
                TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(17))
            ),
        };

        var slots = new List<WeeklyScheduleSlot>
        {
            new(DayOfWeek.Monday, TimeSpan.FromHours(8), TimeSpan.FromHours(13)),
            new(DayOfWeek.Wednesday, TimeSpan.FromHours(8), TimeSpan.FromHours(13)),
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
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(17))
        );
        inactiveSchedule.Deactivate();

        var existingSchedules = new List<Schedule> { inactiveSchedule };

        var slots = new List<WeeklyScheduleSlot>
        {
            new(DayOfWeek.Monday, TimeSpan.FromHours(9), TimeSpan.FromHours(17)),
        };

        // Act
        var result = WeeklyScheduleSetupService.SetupWeeklySchedule(
            doctorId,
            existingSchedules,
            slots
        );

        // Assert
        result.Should().HaveCount(1);
        result[0].DayOfWeek.Should().Be(DayOfWeek.Monday);
        result[0].IsActive.Should().BeTrue();
    }
}
