using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class ScheduleTests
{
    [Fact]
    public void Create_ShouldCreateSchedule_WhenValidParameters()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var dayOfWeek = DayOfWeek.Monday;
        var timeRange = TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(17));

        // Act
        var schedule = Schedule.Create(doctorId, dayOfWeek, timeRange);

        // Assert
        schedule.Should().NotBeNull();
        schedule.DoctorId.Should().Be(doctorId);
        schedule.DayOfWeek.Should().Be(dayOfWeek);
        schedule.TimeRange.Should().Be(timeRange);
        schedule.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(
        "00000000-0000-0000-0000-000000000000",
        DayOfWeek.Monday,
        9,
        17,
        DomainErrors.Validation.ValueRequired
    )]
    [InlineData(
        "11111111-1111-1111-1111-111111111111",
        (DayOfWeek)99,
        9,
        17,
        DomainErrors.Schedule.InvalidDayOfWeek
    )]
    public void Create_ShouldThrowException_WhenInvalidParameters(
        string doctorIdStr,
        DayOfWeek dayOfWeek,
        double startHour,
        double endHour,
        string expectedMessage
    )
    {
        // Arrange & Act
        var act = () =>
            Schedule.Create(
                Guid.Parse(doctorIdStr),
                dayOfWeek,
                TimeRange.Create(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour))
            );

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () => Schedule.Create(Guid.NewGuid(), DayOfWeek.Monday, null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Theory]
    [InlineData(9, 17, 10, 12, true)] // Requested range is inside schedule
    [InlineData(9, 17, 9, 17, true)] // Requested range matches schedule exactly
    [InlineData(9, 17, 9.5, 16.5, true)] // Requested range is inside schedule
    [InlineData(9, 17, 18, 19, false)] // Requested range is after schedule
    [InlineData(9, 17, 7, 8, false)] // Requested range is before schedule
    [InlineData(9, 17, 8, 10, false)] // Requested range starts before schedule
    [InlineData(9, 17, 16, 18, false)] // Requested range ends after schedule
    public void CoversTimeRange_ShouldReturnExpectedResult(
        double scheduleStart,
        double scheduleEnd,
        double requestedStart,
        double requestedEnd,
        bool expected
    )
    {
        // Arrange
        var schedule = Schedule.Create(
            Guid.NewGuid(),
            DayOfWeek.Monday,
            TimeRange.Create(TimeSpan.FromHours(scheduleStart), TimeSpan.FromHours(scheduleEnd))
        );

        // Act & Assert
        schedule
            .CoversTimeRange(
                TimeRange.Create(
                    TimeSpan.FromHours(requestedStart),
                    TimeSpan.FromHours(requestedEnd)
                )
            )
            .Should()
            .Be(expected);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_WhenScheduleIsActive()
    {
        // Arrange
        var schedule = new ScheduleBuilder().Build();

        // Act
        schedule.Deactivate();

        // Assert
        schedule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldThrowException_WhenScheduleIsAlreadyInactive()
    {
        // Arrange
        var schedule = new ScheduleBuilder().Build();
        schedule.Deactivate();

        // Act & Assert
        schedule
            .Invoking(s => s.Deactivate())
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Schedule.AlreadyInactive);
    }

    [Fact]
    public void Deactivate_ShouldPreventCoversTimeRange_WhenDeactivated()
    {
        // Arrange
        var schedule = new ScheduleBuilder().Build();

        // Act
        schedule.Deactivate();

        // Assert
        schedule
            .CoversTimeRange(TimeRange.Create(TimeSpan.FromHours(10), TimeSpan.FromHours(12)))
            .Should()
            .BeFalse();
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldThrowException_WhenActiveDuplicateExists()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var existingSchedules = new List<Schedule>
        {
            new ScheduleBuilder().WithDoctorId(doctorId).Build(),
        };

        // Act
        var act = () =>
            Schedule.EnsureNoDuplicateDay(existingSchedules, doctorId, DayOfWeek.Monday);

        // Assert
        var exceptionAssertion = act.Should()
            .Throw<ScheduleAlreadyExistsException>()
            .WithMessage(DomainErrors.Schedule.ScheduleAlreadyExists);
        exceptionAssertion.Which.DoctorId.Should().Be(doctorId);
        exceptionAssertion.Which.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldNotThrow_WhenNoDuplicateExists()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var existingSchedules = new List<Schedule>
        {
            new ScheduleBuilder().WithDoctorId(doctorId).Build(),
        };

        // Act
        var act = () =>
            Schedule.EnsureNoDuplicateDay(existingSchedules, doctorId, DayOfWeek.Tuesday);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldNotThrow_WhenDuplicateExistsButIsInactive()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var inactiveSchedule = new ScheduleBuilder().WithDoctorId(doctorId).Build();
        inactiveSchedule.Deactivate();

        var existingSchedules = new List<Schedule> { inactiveSchedule };

        // Act
        var act = () =>
            Schedule.EnsureNoDuplicateDay(existingSchedules, doctorId, DayOfWeek.Monday);

        // Assert
        act.Should().NotThrow();
    }

    private class ScheduleBuilder
    {
        private Guid _doctorId = Guid.NewGuid();

        public ScheduleBuilder WithDoctorId(Guid doctorId)
        {
            _doctorId = doctorId;
            return this;
        }

        public Schedule Build() =>
            Schedule.Create(
                _doctorId,
                DayOfWeek.Monday,
                TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(17))
            );
    }
}
