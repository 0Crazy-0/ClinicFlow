using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Reassignment;
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
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));
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
            CreateSchedule(newDoctorId, newDate.DayOfWeek, 9, 17)
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
                    NewDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
                    NewTimeRange = CreateTimeRange(10, 11),
                },
                CreateSchedule(Guid.NewGuid(), DayOfWeek.Monday, 9, 17)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Reassign_ShouldThrowDomainValidationException_WhenScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                CreateDisplacedAppointment(),
                new AppointmentReassignmentArgs
                {
                    NewDoctorId = Guid.NewGuid(),
                    NewDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
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
                CreateSchedule(Guid.NewGuid(), DayOfWeek.Monday, 9, 17)
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
                    NewDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
                    NewTimeRange = null!,
                },
                CreateSchedule(Guid.NewGuid(), DayOfWeek.Monday, 9, 17)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Reassign_ShouldThrowDoctorNotAvailableException_WhenOutsideSchedule()
    {
        // Arrange
        var appointment = CreateDisplacedAppointment();
        var newDoctorId = Guid.NewGuid();
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));

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
                CreateSchedule(newDoctorId, newDate.DayOfWeek, 9, 17)
            );

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    private Appointment CreateDisplacedAppointment()
    {
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        appointment.MarkAsRequiresReassignment();
        appointment.ClearDomainEvents();

        return appointment;
    }

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(new TimeOnly(startHour, 0), new TimeOnly(endHour, 0));

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        int startHour,
        int endHour
    ) => Schedule.Create(doctorId, dayOfWeek, CreateTimeRange(startHour, endHour));
}
