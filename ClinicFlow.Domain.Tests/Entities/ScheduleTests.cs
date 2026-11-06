using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Schedules;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Entities;

public class ScheduleTests
{
    [Fact]
    public void Create_ShouldCreateSchedule_WhenValidParameters()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var dayOfWeek = DayOfWeek.Monday;
        var timeRange = CreateTimeRange(9, 17);

        // Act
        var schedule = Schedule.Create(doctorId, dayOfWeek, timeRange);

        // Assert
        schedule.Should().NotBeNull();
        schedule.DoctorId.Should().Be(doctorId);
        schedule.Weekday.Should().Be(dayOfWeek);
        schedule.TimeSlot.Should().BeEquivalentTo(timeRange);
        schedule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDoctorIdIsEmpty()
    {
        // Arrange & Act
        var act = () => Schedule.Create(Guid.Empty, DayOfWeek.Monday, CreateTimeRange(9, 17));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDayOfWeekIsNotDefined()
    {
        // Arrange & Act
        var act = () =>
            Schedule.Create(Guid.CreateVersion7(), (DayOfWeek)99, CreateTimeRange(9, 17));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Schedule.InvalidDayOfWeek);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () => Schedule.Create(Guid.CreateVersion7(), DayOfWeek.Monday, null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse_WhenScheduleIsActive()
    {
        // Arrange
        var schedule = CreateSchedule();

        // Act
        schedule.Deactivate();

        // Assert
        schedule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldEmitScheduleDeactivatedEvent_WhenScheduleIsActive()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var schedule = CreateSchedule(doctorId);

        // Act
        schedule.Deactivate();

        // Assert
        var domainEvent = schedule
            .DomainEvents.OfType<ScheduleDeactivatedEvent>()
            .Should()
            .ContainSingle()
            .Which;

        domainEvent.ScheduleId.Should().Be(schedule.Id);
        domainEvent.DoctorId.Should().Be(doctorId);
        domainEvent.Weekday.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void Deactivate_ShouldThrowException_WhenScheduleIsAlreadyInactive()
    {
        // Arrange
        var schedule = CreateSchedule();

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
        var schedule = CreateSchedule();

        // Act
        schedule.Deactivate();

        // Assert
        schedule.CoversTimeRange(CreateTimeRange(10, 12)).Should().BeFalse();
    }

    [Fact]
    public void CoversTimeRange_ShouldThrowException_WhenRequestedRangeIsNull()
    {
        // Arrange
        var schedule = CreateSchedule();
        schedule.Deactivate();

        // Act
        var act = () => schedule.CoversTimeRange(null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void CoversTimeRange_ShouldReturnTrue_WhenRequestedRangeMatchesSlotExactly()
    {
        // Arrange
        var schedule = CreateSchedule();

        // Act & Assert
        schedule.CoversTimeRange(CreateTimeRange(9, 17)).Should().BeTrue();
    }

    [Fact]
    public void CoversTimeRange_ShouldReturnFalse_WhenRequestedRangeStartsBeforeSlot()
    {
        // Arrange
        var schedule = CreateSchedule();

        // Act & Assert
        schedule.CoversTimeRange(CreateTimeRange(8, 10)).Should().BeFalse();
    }

    [Fact]
    public void EnsureDoctorIsAvailable_ShouldNotThrow_WhenDoctorMatchesAndScheduleCoversTimeRange()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var schedule = CreateSchedule(doctorId);

        // Act
        var act = () =>
            schedule.EnsureDoctorIsAvailable(doctorId, DayOfWeek.Monday, CreateTimeRange(10, 12));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureDoctorIsAvailable_ShouldThrowException_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var schedule = CreateSchedule();

        // Act
        var act = () =>
            schedule.EnsureDoctorIsAvailable(Guid.Empty, DayOfWeek.Monday, CreateTimeRange(10, 12));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void EnsureDoctorIsAvailable_ShouldThrowException_WhenDayOfWeekIsNotDefined()
    {
        // Arrange
        var schedule = CreateSchedule();

        // Act
        var act = () =>
            schedule.EnsureDoctorIsAvailable(
                schedule.DoctorId,
                (DayOfWeek)999,
                CreateTimeRange(10, 12)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void EnsureDoctorIsAvailable_ShouldThrowException_WhenTimeRangeIsNull()
    {
        // Arrange
        var schedule = CreateSchedule();

        // Act
        var act = () =>
            schedule.EnsureDoctorIsAvailable(Guid.CreateVersion7(), DayOfWeek.Monday, null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void EnsureDoctorIsAvailable_ShouldThrowDoctorNotAvailableException_WhenDoctorIdDoesNotMatch()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var otherDoctorId = Guid.CreateVersion7();
        var schedule = CreateSchedule(doctorId);

        // Act
        var act = () =>
            schedule.EnsureDoctorIsAvailable(
                otherDoctorId,
                DayOfWeek.Monday,
                CreateTimeRange(10, 12)
            );

        // Assert
        var exceptionAssertion = act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
        exceptionAssertion.Which.DoctorId.Should().Be(otherDoctorId);
        exceptionAssertion.Which.Weekday.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void EnsureDoctorIsAvailable_ShouldThrowDoctorNotAvailableException_WhenScheduleDoesNotCoverTimeRange()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var schedule = CreateSchedule(doctorId);

        // Act
        var act = () =>
            schedule.EnsureDoctorIsAvailable(doctorId, DayOfWeek.Monday, CreateTimeRange(8, 10));

        // Assert
        var exceptionAssertion = act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);

        exceptionAssertion.Which.DoctorId.Should().Be(doctorId);
        exceptionAssertion.Which.Weekday.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldThrowException_WhenActiveDuplicateExists()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var existingSchedules = new List<Schedule> { CreateSchedule(doctorId) };

        // Act
        var act = () =>
            Schedule.EnsureNoDuplicateDay(existingSchedules, doctorId, DayOfWeek.Monday);

        // Assert
        var exceptionAssertion = act.Should()
            .Throw<ScheduleAlreadyExistsException>()
            .WithMessage(DomainErrors.Schedule.ScheduleAlreadyExists);
        exceptionAssertion.Which.DoctorId.Should().Be(doctorId);
        exceptionAssertion.Which.Weekday.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldThrowException_WhenDoctorIdIsEmpty()
    {
        // Arrange & Act
        var act = () => Schedule.EnsureNoDuplicateDay([], Guid.Empty, DayOfWeek.Monday);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldThrowException_WhenDayOfWeekIsNotDefined()
    {
        // Arrange & Act
        var act = () => Schedule.EnsureNoDuplicateDay([], Guid.CreateVersion7(), (DayOfWeek)999);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldNotThrow_WhenNoDuplicateExists()
    {
        // Arrange
        var existingSchedules = new List<Schedule> { CreateSchedule() };

        // Act
        var act = () =>
            Schedule.EnsureNoDuplicateDay(
                existingSchedules,
                Guid.CreateVersion7(),
                DayOfWeek.Tuesday
            );

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldNotThrow_WhenDuplicateExistsButIsInactive()
    {
        // Arrange
        var inactiveSchedule = CreateSchedule();

        inactiveSchedule.Deactivate();

        var existingSchedules = new List<Schedule> { inactiveSchedule };

        // Act
        var act = () =>
            Schedule.EnsureNoDuplicateDay(
                existingSchedules,
                Guid.CreateVersion7(),
                DayOfWeek.Monday
            );

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldNotThrow_WhenSameDoctorHasScheduleOnDifferentDay()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var existingSchedules = new List<Schedule> { CreateSchedule(doctorId) }; //schedule is on Monday

        // Act
        var act = () =>
            Schedule.EnsureNoDuplicateDay(existingSchedules, doctorId, DayOfWeek.Tuesday);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNoDuplicateDay_ShouldNotThrow_WhenDifferentDoctorHasScheduleOnSameDay()
    {
        // Arrange
        var existingSchedules = new List<Schedule> { CreateSchedule() };

        // Act
        var act = () =>
            Schedule.EnsureNoDuplicateDay(
                existingSchedules,
                Guid.CreateVersion7(),
                DayOfWeek.Monday
            );

        // Assert
        act.Should().NotThrow();
    }

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(
            TimeOnly.FromTimeSpan(TimeSpan.FromHours(startHour)),
            TimeOnly.FromTimeSpan(TimeSpan.FromHours(endHour))
        );

    private static Schedule CreateSchedule() =>
        Schedule.Create(Guid.CreateVersion7(), DayOfWeek.Monday, CreateTimeRange(9, 17));

    private static Schedule CreateSchedule(Guid doctorId) =>
        Schedule.Create(doctorId, DayOfWeek.Monday, CreateTimeRange(9, 17));
}
