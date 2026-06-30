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
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Scheduling;

public class ScheduleByPatientTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                null!,
                CreateValidPatientSchedulingArgs(),
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(Guid.NewGuid(), DayOfWeek.Monday),
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateAppointmentType(),
                null!,
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(Guid.NewGuid(), DayOfWeek.Monday),
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateAppointmentType(),
                CreateValidPatientSchedulingArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateAppointmentType(),
                CreateValidPatientSchedulingArgs() with
                {
                    TargetPatient = null!,
                },
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(Guid.NewGuid(), DayOfWeek.Monday),
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenInitiatorPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateAppointmentType(),
                CreateValidPatientSchedulingArgs() with
                {
                    InitiatorPatient = null!,
                },
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(Guid.NewGuid(), DayOfWeek.Monday),
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateAppointmentType(),
                CreateValidPatientSchedulingArgs() with
                {
                    TimeRange = null!,
                },
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(Guid.NewGuid(), DayOfWeek.Monday),
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateAppointmentType(),
                CreateValidPatientSchedulingArgs(),
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(Guid.NewGuid(), DayOfWeek.Monday),
                },
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowUnauthorized_WhenUserIdMismatches()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var initiator = CreateSelfPatient();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                appointmentType,
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
    public void ScheduleByPatient_ShouldThrowUnauthorized_WhenNonSelfSchedulesForDifferentPatient()
    {
        // Arrange — family member tries to schedule for a different patient → should be unauthorized
        var userId = Guid.NewGuid();
        var appointmentType = CreateAppointmentType();
        var initiator = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Child"),
            PatientRelationship.Child,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                appointmentType,
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
    public void ScheduleByPatient_ShouldSucceed_WhenNonSelfSchedulesForThemselves()
    {
        // Arrange — family member schedules for themselves → should be allowed
        var userId = Guid.NewGuid();
        var appointmentType = CreateAppointmentType();

        var familyMember = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Spouse"),
            PatientRelationship.Spouse,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        familyMember.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        familyMember.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = familyMember,
            TargetPatient = familyMember,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek),
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            appointmentType,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(familyMember.Id);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowUnauthorized_WhenPhoneIsNotVerified()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = false,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                appointmentType,
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
    public void ScheduleByPatient_ShouldThrowIncompleteProfileException_WhenProfileIncomplete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var appointmentType = CreateAppointmentType();
        var incompletePatient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = incompletePatient,
            TargetPatient = incompletePatient,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                appointmentType,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<IncompleteProfileException>()
            .WithMessage(DomainErrors.Patient.ProfileIncomplete);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowPatientBlockedException_WhenHasPenalties()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
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

        var context = new PatientSchedulingContext
        {
            Penalties = penalties,
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                appointmentType,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<PatientBlockedException>().WithMessage(DomainErrors.Patient.Blocked);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenTooYoung()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, null, false)
        );

        var target = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-15)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                appointmentType,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MinimumAgeNotMet);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDoctorNotAvailableException_WhenNotAvailable()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = TimeRange.Create(new TimeOnly(18, 0), new TimeOnly(19, 0)),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                appointmentType,
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
    public void ScheduleByPatient_ShouldSucceed_WhenAllConditionsMet()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek),
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            appointmentType,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(target.Id);
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeRange.Should().Be(args.TimeRange);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    private PatientSchedulingArgs CreateValidPatientSchedulingArgs() =>
        new()
        {
            TargetPatient = CreateSelfPatient(),
            InitiatorPatient = CreateSelfPatient(),
            TimeRange = CreateTimeRange(),
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

    private static AppointmentTypeDefinition CreateAppointmentType() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            null
        );

    private Patient CreateSelfPatient()
    {
        var patient = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        return patient;
    }
}
