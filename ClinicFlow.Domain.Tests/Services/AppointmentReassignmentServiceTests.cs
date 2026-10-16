using System.Globalization;
using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Reassignment;
using ClinicFlow.Domain.Services.Contexts.Reassignment;
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
        var newDoctorId = Guid.CreateVersion7();
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));
        var newTimeRange = CreateTimeRange(10, 11);

        // Act
        AppointmentReassignmentService.Reassign(
            appointment,
            new AppointmentReassignmentArgs
            {
                NewDoctorId = newDoctorId,
                NewDate = newDate,
                NewTimeSlot = newTimeRange,
            },
            new AppointmentReassignmentContext
            {
                NewDoctorSchedule = CreateSchedule(newDoctorId, newDate.DayOfWeek, 9, 17),
                ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
            }
        );

        // Assert
        appointment.DoctorId.Should().Be(newDoctorId);
        appointment.ScheduledDate.Should().Be(newDate);
        appointment.TimeSlot.Should().Be(newTimeRange);
        appointment.DomainEvents.OfType<AppointmentReassignedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Reassign_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(null!, CreateValidArgs(), CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reassign_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                CreateDisplacedAppointment(),
                null!,
                CreateValidContext()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reassign_ShouldThrowArgumentNullException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                CreateDisplacedAppointment(),
                CreateValidArgs() with
                {
                    NewTimeSlot = null!,
                },
                CreateValidContext()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("args.NewTimeSlot");
    }

    [Fact]
    public void Reassign_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                CreateDisplacedAppointment(),
                CreateValidArgs(),
                null!
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reassign_ShouldThrowArgumentNullException_WhenNewDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                CreateDisplacedAppointment(),
                CreateValidArgs(),
                CreateValidContext() with
                {
                    NewDoctorSchedule = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reassign_ShouldThrowArgumentNullException_WhenClinicTimeZoneIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                CreateDisplacedAppointment(),
                CreateValidArgs(),
                CreateValidContext() with
                {
                    ClinicTimeZone = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("context.ClinicTimeZone");
    }

    [Fact]
    public void Reassign_ShouldThrowBusinessRuleValidationException_WhenSlotIsAmbiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );

        var ambiguousDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var appointment = CreateDisplacedAppointment();
        var newDoctorId = Guid.CreateVersion7();

        // Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                appointment,
                new AppointmentReassignmentArgs
                {
                    NewDoctorId = newDoctorId,
                    NewDate = ambiguousDate,
                    NewTimeSlot = TimeRange.Create(new TimeOnly(1, 0), new TimeOnly(1, 30)),
                },
                new AppointmentReassignmentContext
                {
                    NewDoctorSchedule = CreateSchedule(newDoctorId, ambiguousDate.DayOfWeek, 0, 23),
                    ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
                }
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.AmbiguousTimeSlot);
    }

    [Fact]
    public void Reassign_ShouldThrowDoctorNotAvailableException_WhenOutsideSchedule()
    {
        // Arrange
        var appointment = CreateDisplacedAppointment();
        var newDoctorId = Guid.CreateVersion7();
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));

        // Act
        var act = () =>
            AppointmentReassignmentService.Reassign(
                appointment,
                new AppointmentReassignmentArgs
                {
                    NewDoctorId = newDoctorId,
                    NewDate = newDate,
                    NewTimeSlot = CreateTimeRange(18, 19),
                },
                new AppointmentReassignmentContext
                {
                    NewDoctorSchedule = CreateSchedule(newDoctorId, newDate.DayOfWeek, 9, 17),
                    ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
                }
            );

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    private AppointmentReassignmentArgs CreateValidArgs() =>
        new()
        {
            NewDoctorId = Guid.CreateVersion7(),
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime).AddDays(1),
            NewTimeSlot = CreateTimeRange(10, 11),
        };

    private static AppointmentReassignmentContext CreateValidContext() =>
        new()
        {
            NewDoctorSchedule = CreateSchedule(Guid.CreateVersion7(), DayOfWeek.Monday, 9, 17),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

    private Appointment CreateDisplacedAppointment()
    {
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            Guid.CreateVersion7()
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
