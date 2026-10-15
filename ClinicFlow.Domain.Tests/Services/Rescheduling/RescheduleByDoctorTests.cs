using System.Globalization;
using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Rescheduling;

public class RescheduleByDoctorTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void RescheduleByDoctor_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                null!,
                CreateValidDoctorReschedulingArgs(),
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                CreateAppointment(),
                null!,
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowArgumentNullException_WhenInitiatorDoctorIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                CreateAppointment(),
                CreateValidDoctorReschedulingArgs() with
                {
                    InitiatorDoctor = null!,
                },
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowArgumentNullException_WhenNewTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                CreateAppointment(),
                CreateValidDoctorReschedulingArgs() with
                {
                    NewTimeSlot = null!,
                },
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("args.NewTimeSlot");
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowArgumentNullException_WhenClinicTimeZoneIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                CreateAppointment(),
                CreateValidDoctorReschedulingArgs() with
                {
                    ClinicTimeZone = null!,
                },
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("args.ClinicTimeZone");
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowArgumentNullException_WhenDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                CreateAppointment(),
                CreateValidDoctorReschedulingArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                CreateAppointment(),
                CreateValidDoctorReschedulingArgs(),
                CreateSchedule(),
                null!
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Reschedule.MissingClearance);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowBusinessRuleValidationException_WhenSlotIsAmbiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );

        var ambiguousDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var doctorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(doctorId);
        var doctor = CreateDoctor(doctorId);

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = ambiguousDate,
            NewTimeSlot = TimeRange.Create(new TimeOnly(1, 0), new TimeOnly(1, 30)),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        var doctorSchedule = Schedule.Create(
            doctorId,
            ambiguousDate.DayOfWeek,
            CreateTimeRange(0, 23)
        );

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                appointment,
                args,
                doctorSchedule,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.AmbiguousTimeSlot);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowUnauthorized_WhenDoctorMismatch()
    {
        // Arrange
        var appointment = CreateAppointment();
        var invalidDoctor = CreateDoctor(Guid.CreateVersion7());
        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = invalidDoctor,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(10, 11),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                appointment,
                args,
                doctorSchedule,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldEnforceAvailability_WhenValid()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(doctorId);
        var doctor = CreateDoctor(doctorId);

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(18, 19),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
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
    public void RescheduleByDoctor_ShouldSucceed_WhenValid()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(doctorId);
        var doctor = CreateDoctor(doctorId);

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(10, 11),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        var doctorSchedule = Schedule.Create(
            doctorId,
            args.NewDate.DayOfWeek,
            CreateTimeRange(9, 17)
        );

        // Act
        AppointmentReschedulingService.RescheduleByDoctor(
            appointment,
            args,
            doctorSchedule,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentRescheduledEvent>().Should().ContainSingle();
        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeSlot.Should().Be(args.NewTimeSlot);
    }

    private static DoctorReschedulingArgs CreateValidDoctorReschedulingArgs() =>
        new()
        {
            InitiatorDoctor = CreateDoctor(Guid.CreateVersion7()),
            NewTimeSlot = CreateTimeRange(10, 11),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(new TimeOnly(startHour, 0), new TimeOnly(endHour, 0));

    private static Schedule CreateSchedule() =>
        Schedule.Create(Guid.CreateVersion7(), DayOfWeek.Monday, CreateTimeRange(9, 17));

    private static Doctor CreateDoctor(Guid id)
    {
        var doctor = Doctor.Create(
            Guid.CreateVersion7(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.CreateVersion7(),
            "555-0000",
            ConsultationRoom.Create(1, "Room A", 1)
        );
        doctor.SetId(id);
        return doctor;
    }

    private Appointment CreateAppointment(Guid doctorId)
    {
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            doctorId,
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            Guid.CreateVersion7()
        );

        appointment.ClearDomainEvents();

        return appointment;
    }

    private Appointment CreateAppointment()
    {
        var appointment = Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            Guid.CreateVersion7()
        );

        appointment.ClearDomainEvents();

        return appointment;
    }
}
