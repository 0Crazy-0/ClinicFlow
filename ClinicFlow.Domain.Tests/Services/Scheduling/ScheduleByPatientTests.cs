using System.Globalization;
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
    public void ScheduleByPatient_ShouldThrowArgumentNullException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateValidArgs(),
                CreateValidContext() with
                {
                    AppointmentType = null!,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                null!,
                CreateValidContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowArgumentNullException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateValidArgs(),
                CreateValidContext() with
                {
                    TargetPatient = null!,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowArgumentNullException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateValidArgs() with
                {
                    RequestedTimeSlot = null!,
                },
                CreateValidContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("args.RequestedTimeSlot");
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateValidArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowArgumentNullException_WhenContextDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateValidArgs(),
                CreateValidContext() with
                {
                    DoctorSchedule = null!,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowArgumentNullException_WhenContextClinicTimeZoneIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateValidArgs(),
                CreateValidContext() with
                {
                    ClinicTimeZone = null!,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("context.ClinicTimeZone");
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateValidArgs(),
                CreateValidContext(),
                null!
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.MissingClearance);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowBusinessRuleValidationException_WhenSlotIsAmbiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );
        var ambiguousDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var target = CreateSelfPatient();
        var initiatorUserId = Guid.CreateVersion7();
        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = ambiguousDate,
            RequestedTimeSlot = TimeRange.Create(new TimeOnly(1, 0), new TimeOnly(1, 30)),
        };
        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            InitiatorUserId = initiatorUserId,
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = Schedule.Create(
                args.DoctorId,
                ambiguousDate.DayOfWeek,
                TimeRange.Create(new TimeOnly(0, 0), new TimeOnly(23, 59))
            ),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.AmbiguousTimeSlot);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowUnauthorized_WhenMembershipDeniesCategory()
    {
        // Arrange
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateViewOnlyMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
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
    public void ScheduleByPatient_ShouldThrowArgumentNullException_WhenInitiatorMembershipIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                CreateValidArgs(),
                CreateValidContext() with
                {
                    InitiatorMembership = null!,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByPatient_ShouldSucceed_WhenNonSelfSchedulesForThemselves()
    {
        // Arrange, family member schedules for themselves → should be allowed
        var appointmentType = CreateAppointmentType();

        var familyMember = Patient.CreateProfile(
            PersonName.Create("Spouse"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        familyMember.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        familyMember.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var context = new PatientSchedulingContext
        {
            TargetPatient = familyMember,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = doctorSchedule,
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(familyMember.Id);
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeSlot.Should().Be(args.RequestedTimeSlot);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.ScheduledByUserId.Should().Be(context.InitiatorUserId);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowUnauthorized_WhenPhoneIsNotVerified()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = false,
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
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
        var userId = Guid.CreateVersion7();
        var appointmentType = CreateAppointmentType();
        var incompletePatient = Patient.CreateProfile(
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = incompletePatient,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
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
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
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
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            Penalties = penalties,
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
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
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, null, false)
        );

        var target = Patient.CreateProfile(
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-15)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
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
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenGuardianTriesViaPatientRoute()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Pediatric Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(0, 17, requiresLegalGuardian: true)
        );

        var target = Patient.CreateProfile(
            PersonName.Create("Child"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = doctorSchedule,
            InitiatorMembership = CreateParentMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.LegalGuardianRequired);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenInitiatorLacksLegalAuthorityAndTargetIsMinor()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Pediatric Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(0, 17, requiresLegalGuardian: true)
        );

        var initiator = CreateSelfPatient();
        var target = Patient.CreateProfile(
            PersonName.Create("Child"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = doctorSchedule,
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.LegalGuardianRequired);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDoctorNotAvailableException_WhenNotAvailable()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = TimeRange.Create(new TimeOnly(18, 0), new TimeOnly(19, 0)),
        };

        var scheduleForDifferentDoctor = CreateSchedule();

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = scheduleForDifferentDoctor,
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
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
    public void ScheduleByPatient_ShouldSetPatientNotes_WhenPatientNotesProvided()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            PatientNotes = "Patient note",
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = doctorSchedule,
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.PatientNotes.Should().Be(args.PatientNotes);
        appointment.PatientId.Should().Be(target.Id);
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeSlot.Should().Be(args.RequestedTimeSlot);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.ScheduledByUserId.Should().Be(context.InitiatorUserId);
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void ScheduleByPatient_ShouldLeavePatientNotesEmpty_WhenPatientNotesIsNull()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            PatientNotes = null,
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = doctorSchedule,
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.PatientNotes.Should().BeEmpty();
        appointment.PatientId.Should().Be(target.Id);
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeSlot.Should().Be(args.RequestedTimeSlot);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.ScheduledByUserId.Should().Be(context.InitiatorUserId);
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void ScheduleByPatient_ShouldSucceed_WhenAllConditionsMet()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new PatientSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = doctorSchedule,
            InitiatorMembership = CreateSelfMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
        appointment.PatientId.Should().Be(target.Id);
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeSlot.Should().Be(args.RequestedTimeSlot);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.ScheduledByUserId.Should().Be(context.InitiatorUserId);
        appointment.PatientNotes.Should().BeEmpty();
    }

    private FamilyMembership CreateSelfMembership() =>
        FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private FamilyMembership CreateParentMembership() =>
        FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Parent,
            LegalAuthorityType.Parent,
            FamilyMembershipAccessLevel.Full,
            10,
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private FamilyMembership CreateViewOnlyMembership() =>
        FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.ViewOnly,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private PatientSchedulingArgs CreateValidArgs() =>
        new()
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            PatientNotes = "Note",
        };

    private PatientSchedulingContext CreateValidContext() =>
        new()
        {
            TargetPatient = CreateSelfPatient(),
            AppointmentType = CreateAppointmentType(),
            InitiatorUserId = Guid.CreateVersion7(),
            IsInitiatorPhoneVerified = true,
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

    private static TimeRange CreateTimeRange() =>
        TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(11, 0));

    private static Schedule CreateSchedule() =>
        Schedule.Create(
            Guid.CreateVersion7(),
            DayOfWeek.Monday,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

    private static AppointmentTypeDefinition CreateAppointmentType() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            null
        );

    private Patient CreateSelfPatient()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        return patient;
    }
}
