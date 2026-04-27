using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Rescheduling;

public class RescheduleByDoctorTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void RescheduleByDoctor_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                null!,
                CreateValidDoctorReschedulingArgs(),
                new AppointmentReschedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowDomainValidationException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                CreateAppointment(Guid.NewGuid()),
                null!,
                new AppointmentReschedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowDomainValidationException_WhenInitiatorDoctorIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                CreateAppointment(Guid.NewGuid()),
                CreateValidDoctorReschedulingArgs() with
                {
                    InitiatorDoctor = null!,
                },
                new AppointmentReschedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowDomainValidationException_WhenNewTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(
                CreateAppointment(Guid.NewGuid()),
                CreateValidDoctorReschedulingArgs() with
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
    public void RescheduleByDoctor_ShouldThrowUnauthorized_WhenDoctorMismatch()
    {
        // Arrange
        var appointment = CreateAppointment(Guid.NewGuid());

        var invalidDoctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid());
        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = invalidDoctor,
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
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var appointment = CreateAppointment(doctorId);

        var doctor = CreateDoctor(doctorId, Guid.NewGuid());
        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = true,
        };

        var context = new AppointmentReschedulingContext { HasConflict = true };

        // Act
        AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

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
    public void RescheduleByDoctor_ShouldEnforceAvailability_WhenNotOverbook()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var appointment = CreateAppointment(doctorId);
        var doctor = CreateDoctor(doctorId, Guid.NewGuid());

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19), // 6pm - 7pm
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(doctorId, args.NewDate.DayOfWeek, 9, 17), // 9am - 5pm
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowConflict_WhenNotOverbookAndHasConflict()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var appointment = CreateAppointment(doctorId);
        var doctor = CreateDoctor(doctorId, Guid.NewGuid());

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(doctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldSucceed_WhenValid()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var appointment = CreateAppointment(doctorId);
        var doctor = CreateDoctor(doctorId, Guid.NewGuid());

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(doctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentRescheduledEvent>();

        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    private static DoctorReschedulingArgs CreateValidDoctorReschedulingArgs() =>
        new()
        {
            InitiatorDoctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid()),
            NewTimeRange = CreateTimeRange(10, 11),
        };

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour));

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        int startHour,
        int endHour
    ) => Schedule.Create(doctorId, dayOfWeek, CreateTimeRange(startHour, endHour));

    private static Doctor CreateDoctor(Guid id, Guid userId)
    {
        var doctor = Doctor.Create(
            userId,
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "555-0000",
            ConsultationRoom.Create(101, "Room A", 1)
        );
        doctor.SetId(id);
        return doctor;
    }

    private Appointment CreateAppointment(Guid doctorId)
    {
        var appointmentDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date;

        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            doctorId,
            Guid.NewGuid(),
            appointmentDate,
            TimeRange.Create(
                appointmentDate.TimeOfDay,
                appointmentDate.TimeOfDay.Add(TimeSpan.FromHours(1))
            )
        );

        // Clear construction events for test isolation
        appointment.ClearDomainEvents();

        return appointment;
    }
}
