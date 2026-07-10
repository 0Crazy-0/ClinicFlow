using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
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
                new PatientReschedulingContext { DoctorSchedule = CreateSchedule() },
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
                CreateAppointment(),
                null!,
                new PatientReschedulingContext { DoctorSchedule = CreateSchedule() },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowDomainValidationException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(),
                CreateValidPatientReschedulingArgs(),
                null!,
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
                CreateAppointment(),
                CreateValidPatientReschedulingArgs() with
                {
                    TargetPatient = null!,
                },
                new PatientReschedulingContext { DoctorSchedule = CreateSchedule() },
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
                CreateAppointment(),
                CreateValidPatientReschedulingArgs() with
                {
                    InitiatorPatient = null!,
                },
                new PatientReschedulingContext { DoctorSchedule = CreateSchedule() },
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
                CreateAppointment(),
                CreateValidPatientReschedulingArgs() with
                {
                    NewTimeRange = null!,
                },
                new PatientReschedulingContext { DoctorSchedule = CreateSchedule() },
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
                CreateAppointment(),
                CreateValidPatientReschedulingArgs(),
                new PatientReschedulingContext { DoctorSchedule = CreateSchedule() },
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
        var appointment = CreateAppointment();
        var target = CreateSelfPatient();
        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext { DoctorSchedule = CreateSchedule() };

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
        var target = CreateSelfPatient();
        var initiator = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext { DoctorSchedule = CreateSchedule() };

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
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenNonSelfReschedulesForDifferentPatient()
    {
        // same UserId but different Patient.Id → must still throw Unauthorized
        var initiator = Patient.CreateFamilyMember(
            Guid.CreateVersion7(),
            PersonName.Create("Child"),
            PatientRelationship.Child,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext { DoctorSchedule = CreateSchedule() };

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
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenPhoneIsNotVerified()
    {
        // Arrange
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = false,
        };

        var context = new PatientReschedulingContext { DoctorSchedule = CreateSchedule() };

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
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var penalties = new[]
        {
            PatientPenalty.CreateAutomaticBlock(
                target.Id,
                "Reason",
                BlockDuration.Minor,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        var context = new PatientReschedulingContext
        {
            Penalties = penalties,
            DoctorSchedule = CreateSchedule(),
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
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = TimeRange.Create(new TimeOnly(18, 0), new TimeOnly(19, 0)),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
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
    public void RescheduleByPatient_ShouldSucceed_WhenAllConditionsMet()
    {
        // Arrange
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
        };

        // Act
        AppointmentReschedulingService.RescheduleByPatient(
            appointment,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentRescheduledEvent>().Should().ContainSingle();
        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    [Fact]
    public void RescheduleByPatient_ShouldUpdatePatientNotes_WhenNewPatientNotesIsNotNull()
    {
        // Arrange
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);

        appointment.UpdatePatientNotes("Original notes");

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            NewPatientNotes = "Rescheduled notes",
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
        };

        // Act
        AppointmentReschedulingService.RescheduleByPatient(
            appointment,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.PatientNotes.Should().Be(args.NewPatientNotes);
    }

    [Fact]
    public void RescheduleByPatient_ShouldNotUpdatePatientNotes_WhenNewPatientNotesIsNull()
    {
        // Arrange
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);

        appointment.UpdatePatientNotes("Original notes");

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            NewPatientNotes = null,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
        };

        // Act
        AppointmentReschedulingService.RescheduleByPatient(
            appointment,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.PatientNotes.Should().Be("Original notes");
    }

    private PatientReschedulingArgs CreateValidPatientReschedulingArgs() =>
        new()
        {
            TargetPatient = CreateSelfPatient(),
            InitiatorPatient = CreateSelfPatient(),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

    private static TimeRange CreateTimeRange() =>
        TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0));

    private static Schedule CreateSchedule(Guid doctorId, DayOfWeek dayOfWeek) =>
        Schedule.Create(
            doctorId,
            dayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

    private static Schedule CreateSchedule() =>
        Schedule.Create(
            Guid.CreateVersion7(),
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

    private Patient CreateSelfPatient()
    {
        var dateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30));
        var patient = Patient.CreateSelf(
            Guid.CreateVersion7(),
            PersonName.Create("Test"),
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        return patient;
    }

    private Appointment CreateAppointment(Guid patientId)
    {
        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
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
