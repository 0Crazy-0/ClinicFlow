using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Rescheduling;

public class RescheduleByStaffTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void RescheduleByStaff_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(
                null!,
                CreateValidStaffReschedulingArgs(),
                new AppointmentReschedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByStaff_ShouldThrowDomainValidationException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(
                CreateAppointment(),
                null!,
                new AppointmentReschedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByStaff_ShouldThrowDomainValidationException_WhenNewTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(
                CreateAppointment(),
                CreateValidStaffReschedulingArgs() with
                {
                    NewTimeRange = null!,
                },
                new AppointmentReschedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByStaff_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var appointment = CreateAppointment();

        var args = new StaffReschedulingArgs
        {
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = true,
        };

        var context = new AppointmentReschedulingContext { HasConflict = true };

        // Act
        AppointmentReschedulingService.RescheduleByStaff(appointment, args, context);

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentRescheduledEvent>();

        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    [Fact]
    public void RescheduleByStaff_ShouldEnforceAvailability_WhenNotOverbook()
    {
        // Arrange
        var appointment = CreateAppointment();

        var args = new StaffReschedulingArgs
        {
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void RescheduleByStaff_ShouldThrowConflict_WhenNotOverbookAndHasConflict()
    {
        // Arrange
        var appointment = CreateAppointment();

        var args = new StaffReschedulingArgs
        {
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void RescheduleByStaff_ShouldSucceed_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment();

        var args = new StaffReschedulingArgs
        {
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        AppointmentReschedulingService.RescheduleByStaff(appointment, args, context);

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentRescheduledEvent>();

        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    private static StaffReschedulingArgs CreateValidStaffReschedulingArgs() =>
        new() { NewTimeRange = CreateTimeRange(10, 11) };

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour));

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        int startHour,
        int endHour
    ) => Schedule.Create(doctorId, dayOfWeek, CreateTimeRange(startHour, endHour));

    private Appointment CreateAppointment()
    {
        var scheduledDateTime = _fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date;

        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDateTime.Date,
            TimeRange.Create(
                scheduledDateTime.TimeOfDay,
                scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );

        // Clear construction events for test isolation
        appointment.ClearDomainEvents();

        return appointment;
    }
}
