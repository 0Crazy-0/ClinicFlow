using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.ValueObjects;
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
                CreateSchedule(Guid.CreateVersion7(), DayOfWeek.Monday, 9, 17),
                SchedulingClearance.Granted()
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
                CreateSchedule(Guid.CreateVersion7(), DayOfWeek.Monday, 9, 17),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByStaff_ShouldThrowDomainValidationException_WhenDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(
                CreateAppointment(),
                CreateValidStaffReschedulingArgs(),
                null!,
                SchedulingClearance.Granted()
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
                CreateSchedule(Guid.CreateVersion7(), DayOfWeek.Monday, 9, 17),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByStaff_ShouldThrowDomainValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(
                CreateAppointment(),
                CreateValidStaffReschedulingArgs(),
                CreateSchedule(Guid.CreateVersion7(), DayOfWeek.Monday, 9, 17),
                null!
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
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = true,
        };

        var doctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17);

        // Act
        AppointmentReschedulingService.RescheduleByStaff(
            appointment,
            args,
            doctorSchedule,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentRescheduledEvent>().Should().ContainSingle();
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
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = false,
        };

        var doctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17);

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(
                appointment,
                args,
                doctorSchedule,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void RescheduleByStaff_ShouldSucceed_WhenValid()
    {
        // Arrange
        var appointment = CreateAppointment();

        var args = new StaffReschedulingArgs
        {
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var doctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17);

        // Act
        AppointmentReschedulingService.RescheduleByStaff(
            appointment,
            args,
            doctorSchedule,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentRescheduledEvent>().Should().ContainSingle();
        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    private static StaffReschedulingArgs CreateValidStaffReschedulingArgs() =>
        new() { NewTimeRange = CreateTimeRange(10, 11) };

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(new TimeOnly(startHour, 0), new TimeOnly(endHour, 0));

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        int startHour,
        int endHour
    ) => Schedule.Create(doctorId, dayOfWeek, CreateTimeRange(startHour, endHour));

    private Appointment CreateAppointment()
    {
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        appointment.ClearDomainEvents();

        return appointment;
    }
}
