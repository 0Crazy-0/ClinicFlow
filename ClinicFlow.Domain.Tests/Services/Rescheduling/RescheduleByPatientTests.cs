using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Rescheduling;

public class RescheduleByPatientTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void RescheduleByPatient_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                null!,
                CreateValidPatientReschedulingArgs(),
                new AppointmentReschedulingContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowDomainValidationException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(Guid.NewGuid()),
                null!,
                new AppointmentReschedulingContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowDomainValidationException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(Guid.NewGuid()),
                CreateValidPatientReschedulingArgs() with
                {
                    TargetPatient = null!,
                },
                new AppointmentReschedulingContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowDomainValidationException_WhenInitiatorPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(Guid.NewGuid()),
                CreateValidPatientReschedulingArgs() with
                {
                    InitiatorPatient = null!,
                },
                new AppointmentReschedulingContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowDomainValidationException_WhenNewTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(Guid.NewGuid()),
                CreateValidPatientReschedulingArgs() with
                {
                    NewTimeRange = null!,
                },
                new AppointmentReschedulingContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowDomainValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(Guid.NewGuid()),
                CreateValidPatientReschedulingArgs(),
                new AppointmentReschedulingContext(),
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowValidationException_WhenTargetMismatch()
    {
        // Arrange
        var appointment = CreateAppointment(Guid.NewGuid());

        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsInitiatorPhoneVerified = true,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.DataMismatch);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenUserIdMismatches()
    {
        // Arrange
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var initiator = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = CreateAppointment(target.Id);

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsInitiatorPhoneVerified = true,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenNonSelfReschedulesForDifferentPatient()
    {
        // same UserId but different Patient.Id → must still throw Unauthorized
        var userId = Guid.NewGuid();

        var initiator = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Child"),
            Enums.PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-10),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        initiator.SetId(Guid.NewGuid());

        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = CreateAppointment(target.Id);

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsInitiatorPhoneVerified = true,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenPhoneIsNotVerified()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = CreateAppointment(target.Id);

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsInitiatorPhoneVerified = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.PhoneNotVerified);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowPatientBlockedException_WhenHasPenalties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = CreateAppointment(target.Id);

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsInitiatorPhoneVerified = true,
        };

        var penalties = new[]
        {
            PatientPenalty.CreateAutomaticBlock(
                target.Id,
                "Reason",
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(5).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        var context = new AppointmentReschedulingContext
        {
            Penalties = penalties,
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<PatientBlockedException>().WithMessage(DomainErrors.Patient.Blocked);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowDoctorNotAvailableException_WhenNotAvailable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = CreateAppointment(target.Id);

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19), // 6pm - 7pm
            IsInitiatorPhoneVerified = true,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17), // 9am - 5pm
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowAppointmentConflictException_WhenConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = CreateAppointment(target.Id);

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsInitiatorPhoneVerified = true,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void RescheduleByPatient_ShouldSucceed_WhenAllConditionsMet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = CreateAppointment(target.Id);

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsInitiatorPhoneVerified = true,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        AppointmentReschedulingService.RescheduleByPatient(
            appointment,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentRescheduledEvent>();

        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    private PatientReschedulingArgs CreateValidPatientReschedulingArgs() =>
        new()
        {
            TargetPatient = CreateSelfPatient(
                Guid.NewGuid(),
                Guid.NewGuid(),
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            InitiatorPatient = CreateSelfPatient(
                Guid.NewGuid(),
                Guid.NewGuid(),
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            NewTimeRange = CreateTimeRange(10, 11),
            IsInitiatorPhoneVerified = true,
        };

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour));

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        int startHour,
        int endHour
    ) => Schedule.Create(doctorId, dayOfWeek, CreateTimeRange(startHour, endHour));

    private static Patient CreateSelfPatient(Guid id, Guid userId, int age, DateTime referenceTime)
    {
        var dateOfBirth = referenceTime.AddYears(-age);
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            dateOfBirth,
            referenceTime
        );

        patient.SetId(id);
        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        return patient;
    }

    private Appointment CreateAppointment(Guid patientId)
    {
        var scheduledDateTime = _fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date;

        var appointment = Appointment.Schedule(
            patientId,
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
