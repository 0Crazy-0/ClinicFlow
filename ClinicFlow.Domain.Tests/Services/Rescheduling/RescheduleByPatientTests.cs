using System.Globalization;
using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts.Rescheduling;
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
                CreateValidArgs(),
                CreateValidContext(),
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
                CreateValidContext(),
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
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenNewTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(),
                CreateValidArgs() with
                {
                    NewTimeSlot = null!,
                },
                CreateValidContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("args.NewTimeSlot");
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(),
                CreateValidArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(),
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
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenContextDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(),
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
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenInitiatorMembershipIsNull()
    {
        // Arrange
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = CreateValidArgs();
        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = null!,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowArgumentNullException_WhenContextClinicTimeZoneIsNull()
    {
        // Arrange
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = CreateValidArgs();
        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = null!,
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
        act.Should().Throw<ArgumentNullException>().WithParameterName("context.ClinicTimeZone");
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(
                CreateAppointment(),
                CreateValidArgs(),
                CreateValidContext(),
                null!
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Reschedule.MissingClearance);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowBusinessRuleValidationException_WhenSlotIsAmbiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );

        var ambiguousDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);

        var args = new PatientReschedulingArgs
        {
            NewDate = ambiguousDate,
            NewTimeSlot = TimeRange.Create(new TimeOnly(1, 0), new TimeOnly(1, 30)),
        };
        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, ambiguousDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.AmbiguousTimeSlot);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowValidationException_WhenTargetMismatch()
    {
        // Arrange
        var appointment = CreateAppointment();
        var target = CreateSelfPatient();
        var args = new PatientReschedulingArgs
        {
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenMembershipDeniesCategory()
    {
        // Arrange
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                PatientRelationship.Child,
                LegalAuthorityType.None,
                FamilyMembershipAccessLevel.ViewOnly,
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
    public void RescheduleByPatient_ShouldThrowPatientBlockedException_WhenHasPenalties()
    {
        // Arrange
        var target = CreateSelfPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new PatientReschedulingArgs
        {
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
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
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            Penalties = penalties,
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = TimeRange.Create(new TimeOnly(18, 0), new TimeOnly(19, 0)),
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
        appointment.TimeSlot.Should().Be(args.NewTimeSlot);
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
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
            NewPatientNotes = "Rescheduled notes",
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
            NewPatientNotes = null,
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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

        appointment.ClearDomainEvents();

        var args = new PatientReschedulingArgs
        {
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
        appointment.TimeSlot.Should().Be(args.NewTimeSlot);
    }

    [Fact]
    public void RescheduleByPatient_ShouldSucceed_WhenExactlyAdultReschedulesGuardianCreatedAppointment()
    {
        // Arrange
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));
        var target = Patient.CreateProfile(
            PersonName.Create("Test", null, "Test"),
            newDate.AddYears(-FamilyMembership.MinimumAdultAge),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", null, "Name", "1234567890"));

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
            NewDate = newDate,
            NewTimeSlot = CreateTimeRange(),
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
        appointment.TimeSlot.Should().Be(args.NewTimeSlot);
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
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
        appointment.TimeSlot.Should().Be(args.NewTimeSlot);
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
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
        };

        var context = new PatientReschedulingContext
        {
            TargetPatient = target,
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = FamilyMembership.CreateSelf(
                target.Id,
                minorId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
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
        appointment.TimeSlot.Should().Be(args.NewTimeSlot);
    }

    private Patient CreateMinorPatient()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Child", null, "Child"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", null, "Name", "1234567890"));

        return patient;
    }

    private FamilyMembership CreateSelfMembership() =>
        FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private PatientReschedulingArgs CreateValidArgs() =>
        new()
        {
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeSlot = CreateTimeRange(),
        };

    private PatientReschedulingContext CreateValidContext() =>
        new()
        {
            TargetPatient = CreateSelfPatient(),
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

    private static AppointmentTypeDefinition CreateAppointmentType() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            AppointmentPurpose.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            null
        );

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
            PersonName.Create("Test", null, "Test"),
            dateOfBirth,
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", null, "Name", "1234567890"));

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
