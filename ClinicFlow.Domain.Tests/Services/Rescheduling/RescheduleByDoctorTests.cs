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
    public void RescheduleByDoctor_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
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
                CreateAppointment(),
                null!,
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowDomainValidationException_WhenDoctorScheduleIsNull()
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
                CreateAppointment(),
                CreateValidDoctorReschedulingArgs() with
                {
                    InitiatorDoctor = null!,
                },
                CreateSchedule(),
                SchedulingClearance.Granted()
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
                CreateAppointment(),
                CreateValidDoctorReschedulingArgs() with
                {
                    NewTimeRange = null!,
                },
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowDomainValidationException_WhenClearanceIsNull()
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
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
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
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
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
    public void RescheduleByDoctor_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(doctorId);

        var doctor = CreateDoctor(doctorId);
        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = true,
        };

        var doctorSchedule = CreateSchedule();

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
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldEnforceAvailability_WhenNotOverbook()
    {
        // Arrange
        var doctorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(doctorId);
        var doctor = CreateDoctor(doctorId);

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = false,
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
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
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
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    private static DoctorReschedulingArgs CreateValidDoctorReschedulingArgs() =>
        new()
        {
            InitiatorDoctor = CreateDoctor(Guid.CreateVersion7()),
            NewTimeRange = CreateTimeRange(10, 11),
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
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
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
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0))
        );

        appointment.ClearDomainEvents();

        return appointment;
    }
}
