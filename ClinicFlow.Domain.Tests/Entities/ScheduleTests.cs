using ClinicFlow.Domain.Entities;
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
        var timeRange = new TimeRange(TimeSpan.FromHours(9), TimeSpan.FromHours(17));

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
    [InlineData("00000000-0000-0000-0000-000000000000", DayOfWeek.Monday, 9, 17, "Doctor ID cannot be empty.")]
    [InlineData("11111111-1111-1111-1111-111111111111", (DayOfWeek)99, 9, 17, "Invalid day of the week.")]
    public void Create_ShouldThrowException_WhenInvalidParameters(string doctorIdStr, DayOfWeek dayOfWeek, double startHour, double endHour, string expectedMessage)
    {
        // Arrange & Act
        var act = () => Schedule.Create(Guid.Parse(doctorIdStr), dayOfWeek, new TimeRange(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour)));

        // Assert
        act.Should().Throw<InvalidScheduleException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () => Schedule.Create(Guid.NewGuid(), DayOfWeek.Monday, null!);

        // Assert
        act.Should().Throw<InvalidScheduleException>().WithMessage("Time range cannot be null.");
    }

    [Theory]
    [InlineData(9, 17, 10, 12, true)] // Requested range is inside schedule
    [InlineData(9, 17, 9, 17, true)] // Requested range matches schedule exactly
    [InlineData(9, 17, 9.5, 16.5, true)] // Requested range is inside schedule
    [InlineData(9, 17, 18, 19, false)] // Requested range is after schedule
    [InlineData(9, 17, 7, 8, false)] // Requested range is before schedule
    [InlineData(9, 17, 8, 10, false)] // Requested range starts before schedule
    [InlineData(9, 17, 16, 18, false)] // Requested range ends after schedule
    public void CoversTimeRange_ShouldReturnExpectedResult(double scheduleStart, double scheduleEnd, double requestedStart, double requestedEnd, bool expected)
    {
        // Arrange
        var schedule = Schedule.Create(Guid.NewGuid(), DayOfWeek.Monday, new TimeRange(TimeSpan.FromHours(scheduleStart), TimeSpan.FromHours(scheduleEnd)));

        // Act & Assert
        schedule.CoversTimeRange(new TimeRange(TimeSpan.FromHours(requestedStart), TimeSpan.FromHours(requestedEnd))).Should().Be(expected);

    }
}
