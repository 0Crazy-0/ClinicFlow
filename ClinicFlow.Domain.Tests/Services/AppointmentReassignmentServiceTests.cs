using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Reassignment;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentReassignmentServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Reassign_ShouldSucceed_WhenDoctorIsAvailableAndNoConflict()
    {
        // Arrange
        var appointment = CreateDisplacedAppointment();
        var newDoctorId = Guid.NewGuid();
        var newDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date;
        var newTimeRange = CreateTimeRange(10, 11);

        // Act
        AppointmentReassignmentService.Reassign(
            appointment,
            new AppointmentReassignmentArgs
            {
                NewDoctorId = newDoctorId,
                NewDate = newDate,
                NewTimeRange = newTimeRange,
            },
            new AppointmentReassignmentContext
            {
                NewDoctorSchedule = CreateSchedule(newDoctorId, newDate.DayOfWeek, 9, 17),
                HasConflict = false,
            }
        );

        // Assert
        appointment.DoctorId.Should().Be(newDoctorId);
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeRange.Should().Be(newTimeRange);
        appointment.DomainEvents.OfType<AppointmentReassignedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Reassign_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                null!,
                new AppointmentReassignmentArgs
                {
                    NewDoctorId = Guid.NewGuid(),
                    NewDate = DateTime.UtcNow.AddDays(1),
                    NewTimeRange = CreateTimeRange(10, 11),
                },
                new AppointmentReassignmentContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Reassign_ShouldThrowDomainValidationException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                CreateDisplacedAppointment(),
                new AppointmentReassignmentArgs
                {
                    NewDoctorId = Guid.NewGuid(),
                    NewDate = DateTime.UtcNow.AddDays(1).Date,
                    NewTimeRange = CreateTimeRange(10, 11),
                },
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Reassign_ShouldThrowDomainValidationException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                CreateDisplacedAppointment(),
                null!,
                new AppointmentReassignmentContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Reassign_ShouldThrowDomainValidationException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                CreateDisplacedAppointment(),
                new AppointmentReassignmentArgs
                {
                    NewDoctorId = Guid.NewGuid(),
                    NewDate = DateTime.UtcNow.AddDays(1),
                    NewTimeRange = null!,
                },
                new AppointmentReassignmentContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Reassign_ShouldThrowDoctorNotAvailableException_WhenScheduleIsNull()
    {
        // Arrange
        var appointment = CreateDisplacedAppointment();
        var newDoctorId = Guid.NewGuid();
        var newDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date;

        // Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                appointment,
                new AppointmentReassignmentArgs
                {
                    NewDoctorId = newDoctorId,
                    NewDate = newDate,
                    NewTimeRange = CreateTimeRange(10, 11),
                },
                new AppointmentReassignmentContext { NewDoctorSchedule = null, HasConflict = false }
            );

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void Reassign_ShouldThrowDoctorNotAvailableException_WhenOutsideSchedule()
    {
        // Arrange
        var appointment = CreateDisplacedAppointment();
        var newDoctorId = Guid.NewGuid();
        var newDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date;

        // Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                appointment,
                new AppointmentReassignmentArgs
                {
                    NewDoctorId = newDoctorId,
                    NewDate = newDate,
                    NewTimeRange = CreateTimeRange(18, 19),
                },
                new AppointmentReassignmentContext
                {
                    NewDoctorSchedule = CreateSchedule(newDoctorId, newDate.DayOfWeek, 9, 17),
                    HasConflict = false,
                }
            );

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void Reassign_ShouldThrowConflictException_WhenHasConflict()
    {
        // Arrange
        var appointment = CreateDisplacedAppointment();
        var newDoctorId = Guid.NewGuid();
        var newDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date;

        var context = new AppointmentReassignmentContext
        {
            NewDoctorSchedule = CreateSchedule(newDoctorId, newDate.DayOfWeek, 9, 17),
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                appointment,
                new AppointmentReassignmentArgs
                {
                    NewDoctorId = newDoctorId,
                    NewDate = newDate,
                    NewTimeRange = CreateTimeRange(10, 11),
                },
                context
            );

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    private Appointment CreateDisplacedAppointment()
    {
        var scheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date;

        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            scheduledDate,
            TimeRange.Create(TimeSpan.FromHours(9), TimeSpan.FromHours(10))
        );

        appointment.MarkAsRequiresReassignment();
        appointment.ClearDomainEvents();

        return appointment;
    }

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour));

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        int startHour,
        int endHour
    ) => Schedule.Create(doctorId, dayOfWeek, CreateTimeRange(startHour, endHour));
}
