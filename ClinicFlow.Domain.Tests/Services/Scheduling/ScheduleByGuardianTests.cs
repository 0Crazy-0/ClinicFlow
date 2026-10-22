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
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Scheduling;

public class ScheduleByGuardianTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                null!,
                CreateValidContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenInitiatorMembershipIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateValidArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenContextDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenContextClinicTimeZoneIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                new GuardianSchedulingArgs { RequestedTimeSlot = CreateTimeRange() },
                new PatientSchedulingContext
                {
                    TargetPatient = CreateMinorPatient(),
                    DoctorSchedule = CreateSchedule(),
                    InitiatorMembership = CreateSelfMembership(),
                    AppointmentType = CreateAppointmentType(),
                    ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
                },
                null!
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.MissingClearance);
    }

    [Fact]
    public void ScheduleByGuardian_ShouldThrowBusinessRuleValidationException_WhenSlotIsAmbiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );

        var ambiguousDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var appointmentType = CreateAppointmentType();
        var target = CreateMinorPatient();

        var args = new GuardianSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = ambiguousDate,
            RequestedTimeSlot = TimeRange.Create(new TimeOnly(1, 0), new TimeOnly(1, 30)),
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            DoctorSchedule = Schedule.Create(
                args.DoctorId,
                ambiguousDate.DayOfWeek,
                TimeRange.Create(new TimeOnly(0, 0), new TimeOnly(23, 59))
            ),
            InitiatorMembership = CreateParentMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowUnauthorized_WhenInitiatorIsNotGuardian()
    {
        // Arrange
        var args = new GuardianSchedulingArgs { RequestedTimeSlot = CreateTimeRange() };

        var context = new PatientSchedulingContext
        {
            TargetPatient = CreateMinorPatient(),
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(), // Guardian is self
            AppointmentType = CreateAppointmentType(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowIncompleteProfileException_WhenProfileIncomplete()
    {
        // Arrange
        var incompletePatient = Patient.CreateProfile(
            PersonName.Create("Child"),
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

        var args = new GuardianSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = incompletePatient,
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
            AppointmentType = CreateAppointmentType(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowPatientBlockedException_WhenHasPenalties()
    {
        // Arrange
        var target = CreateMinorPatient();
        var args = new GuardianSchedulingArgs
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
            Penalties = penalties,
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
            AppointmentType = CreateAppointmentType(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<PatientBlockedException>().WithMessage(DomainErrors.Patient.Blocked);
    }

    [Fact]
    public void ScheduleByGuardian_ShouldThrowDomainValidationException_WhenTooYoung()
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

        var args = new GuardianSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = CreateMinorPatient(),
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowDoctorNotAvailableException_WhenNotAvailable()
    {
        // Arrange
        var args = new GuardianSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = TimeRange.Create(new TimeOnly(18, 0), new TimeOnly(19, 0)),
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = CreateMinorPatient(),
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
            AppointmentType = CreateAppointmentType(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowValidation_WhenPatientIsAdult()
    {
        // Arrange
        var patientAdult = Patient.CreateProfile(
            PersonName.Create("Adult"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-25)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patientAdult.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patientAdult.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new GuardianSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            GuardianNotes = null,
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = patientAdult,
            DoctorSchedule = Schedule.Create(
                args.DoctorId,
                args.ScheduledDate.DayOfWeek,
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
            ),
            InitiatorMembership = CreateParentMembership(),
            AppointmentType = CreateAppointmentType(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.GuardianRequiresMinor);
    }

    [Fact]
    public void ScheduleByGuardian_ShouldThrowValidation_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var scheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1));
        var target = Patient.CreateProfile(
            PersonName.Create("Child"),
            scheduledDate.AddYears(-FamilyMembership.MinimumAdultAge),
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
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new GuardianSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = scheduledDate,
            RequestedTimeSlot = CreateTimeRange(),
            GuardianNotes = null,
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            DoctorSchedule = Schedule.Create(
                args.DoctorId,
                args.ScheduledDate.DayOfWeek,
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
            ),
            InitiatorMembership = CreateParentMembership(),
            AppointmentType = CreateAppointmentType(),
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.GuardianRequiresMinor);
    }

    [Fact]
    public void ScheduleByGuardian_ShouldUpdateGuardianNotesAndAuthor_WhenValid()
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

        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var args = new GuardianSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            GuardianNotes = "notes",
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            DoctorSchedule = doctorSchedule,
            InitiatorMembership = FamilyMembership.CreateFamilyMember(
                target.Id,
                authorId,
                PatientRelationship.Parent,
                LegalAuthorityType.Parent,
                FamilyMembershipAccessLevel.Full,
                10,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByGuardian(
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.PatientNotes.Should().BeEmpty();
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.AppointmentTypeId.Should().Be(appointmentType.Id);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeSlot.Should().Be(args.RequestedTimeSlot);
        appointment.ScheduledByUserId.Should().Be(authorId);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
        appointment.GuardianNotes.Should().Be(args.GuardianNotes);
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
    }

    [Fact]
    public void ScheduleByGuardian_ShouldSucceedWithoutGuardianNotes_WhenNullProvided()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var args = new GuardianSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            GuardianNotes = null,
        };

        var context = new PatientSchedulingContext
        {
            TargetPatient = target,
            DoctorSchedule = Schedule.Create(
                args.DoctorId,
                args.ScheduledDate.DayOfWeek,
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
            ),
            InitiatorMembership = FamilyMembership.CreateFamilyMember(
                target.Id,
                authorId,
                PatientRelationship.Parent,
                LegalAuthorityType.Parent,
                FamilyMembershipAccessLevel.Full,
                10,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            AppointmentType = appointmentType,
            ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByGuardian(
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.PatientNotes.Should().BeEmpty();
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.AppointmentTypeId.Should().Be(appointmentType.Id);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeSlot.Should().Be(args.RequestedTimeSlot);
        appointment.ScheduledByUserId.Should().Be(authorId);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
        appointment.GuardianNotes.Should().BeEmpty();
        appointment.GuardianNotesAuthorUserId.Should().BeNull();
    }

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

    private FamilyMembership CreateSelfMembership() =>
        FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private Patient CreateMinorPatient()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Child"),
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
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        return patient;
    }

    private GuardianSchedulingArgs CreateValidArgs() =>
        new()
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            GuardianNotes = "Note",
        };

    private PatientSchedulingContext CreateValidContext() =>
        new()
        {
            TargetPatient = CreateMinorPatient(),
            AppointmentType = CreateAppointmentType(),
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
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
}
