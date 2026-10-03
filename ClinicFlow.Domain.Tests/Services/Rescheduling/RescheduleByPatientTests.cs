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
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenArgsIsNull()
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenTargetPatientIsNull()
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenNewTimeRangeIsNull()
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenContextIsNull()
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenContextDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(),
                CreateValidPatientReschedulingArgs(),
                new PatientReschedulingContext { DoctorSchedule = null! },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
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
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Reschedule.MissingClearance);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowValidationException_WhenTargetMismatch()
    {
        // Arrange
        var appointment = CreateAppointment();
        var target = CreateSelfPatient();
        var args = new PatientReschedulingArgs
        {
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            InitiatorUserId = Guid.CreateVersion7(),
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(),
            InitiatorHasAccessToTarget = true,
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
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenInitiatorHasAccessToTargetIsFalse()
    {
        // Arrange
        var target = CreateSelfPatient();
        var initiator = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            InitiatorUserId = Guid.CreateVersion7(),
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(),
            InitiatorHasAccessToTarget = false,
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
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = false,
            InitiatorUserId = Guid.CreateVersion7(),
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(),
            InitiatorHasAccessToTarget = true,
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
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            InitiatorUserId = Guid.CreateVersion7(),
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
            InitiatorHasAccessToTarget = true,
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
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = TimeRange.Create(new TimeOnly(18, 0), new TimeOnly(19, 0)),
            IsInitiatorPhoneVerified = true,
            InitiatorUserId = Guid.CreateVersion7(),
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorHasAccessToTarget = true,
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
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            InitiatorUserId = Guid.CreateVersion7(),
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorHasAccessToTarget = true,
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
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            InitiatorUserId = Guid.CreateVersion7(),
            NewPatientNotes = "Rescheduled notes",
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorHasAccessToTarget = true,
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
            TargetPatient = target,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            InitiatorUserId = Guid.CreateVersion7(),
            NewPatientNotes = null,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorHasAccessToTarget = true,
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

    [Fact]
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenMinorReschedulesGuardianCreatedAppointment()
    {
        // Arrange
        var target = CreateMinorPatient();
        var guardianId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            target.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            guardianId
        );

        var minorId = Guid.CreateVersion7();
        appointment.ClearDomainEvents();

        var args = new PatientReschedulingArgs
        {
            TargetPatient = target,
            InitiatorUserId = minorId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorHasAccessToTarget = true,
            CreatorLegalAuthority = LegalAuthorityType.Parent,
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
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void RescheduleByPatient_ShouldSucceed_WhenAdultReschedulesGuardianCreatedAppointment()
    {
        // Arrange
        var target = CreateSelfPatient(); // dateOfBirth is 30 years ago
        var guardianId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            target.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            guardianId
        );

        appointment.ClearDomainEvents();

        var args = new PatientReschedulingArgs
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorHasAccessToTarget = true,
            CreatorLegalAuthority = LegalAuthorityType.Parent,
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
    public void RescheduleByPatient_ShouldSucceed_WhenExactlyAdultReschedulesGuardianCreatedAppointment()
    {
        // Arrange
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));
        var target = Patient.CreateProfile(
            PersonName.Create("Test"),
            newDate.AddYears(-FamilyMembership.MinimumAdultAge),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var guardianId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            target.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            guardianId
        );

        appointment.ClearDomainEvents();

        var args = new PatientReschedulingArgs
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            NewDate = newDate,
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorHasAccessToTarget = true,
            CreatorLegalAuthority = LegalAuthorityType.Parent,
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
    public void RescheduleByPatient_ShouldSucceed_WhenGuardianAuthorityExtinguished()
    {
        // Arrange
        var target = CreateMinorPatient();
        var guardianId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            target.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            guardianId
        );

        appointment.ClearDomainEvents();

        var args = new PatientReschedulingArgs
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorHasAccessToTarget = true,
            CreatorLegalAuthority = LegalAuthorityType.None,
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
    public void RescheduleByPatient_ShouldSucceed_WhenReschedulingOwnAppointment()
    {
        // Arrange
        var target = CreateMinorPatient();
        var minorId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            target.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            minorId
        );

        appointment.ClearDomainEvents();

        var args = new PatientReschedulingArgs
        {
            TargetPatient = target,
            InitiatorUserId = minorId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorHasAccessToTarget = true,
            CreatorLegalAuthority = LegalAuthorityType.Parent,
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

    private Patient CreateMinorPatient()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Child"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        return patient;
    }

    private PatientReschedulingArgs CreateValidPatientReschedulingArgs() =>
        new()
        {
            TargetPatient = CreateSelfPatient(),
            InitiatorUserId = Guid.CreateVersion7(),
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
        var patient = Patient.CreateProfile(
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
