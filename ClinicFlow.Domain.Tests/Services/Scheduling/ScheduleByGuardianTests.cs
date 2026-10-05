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

public class ScheduleByGuardianTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                null!,
                CreateValidGuardianSchedulingArgs(),
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(),
                    InitiatorMembership = CreateSelfMembership(),
                    RequestedCategory = AppointmentCategory.Other,
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
                CreateAppointmentType(),
                null!,
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(),
                    InitiatorMembership = CreateSelfMembership(),
                    RequestedCategory = AppointmentCategory.Other,
                },
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
                CreateAppointmentType(),
                CreateValidGuardianSchedulingArgs() with
                {
                    TargetPatient = null!,
                },
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(),
                    InitiatorMembership = CreateSelfMembership(),
                    RequestedCategory = AppointmentCategory.Other,
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
                CreateAppointmentType(),
                CreateValidGuardianSchedulingArgs(),
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(),
                    InitiatorMembership = null!,
                    RequestedCategory = AppointmentCategory.Other,
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
                CreateAppointmentType(),
                CreateValidGuardianSchedulingArgs() with
                {
                    TimeRange = null!,
                },
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(),
                    InitiatorMembership = CreateSelfMembership(),
                    RequestedCategory = AppointmentCategory.Other,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByGuardian_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
                CreateValidGuardianSchedulingArgs(),
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
                CreateAppointmentType(),
                CreateValidGuardianSchedulingArgs(),
                new PatientSchedulingContext
                {
                    DoctorSchedule = null!,
                    InitiatorMembership = CreateSelfMembership(),
                    RequestedCategory = AppointmentCategory.Other,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByGuardian_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
                new GuardianSchedulingArgs
                {
                    TargetPatient = CreateMinorPatient(),
                    InitiatorUserId = Guid.CreateVersion7(),
                    TimeRange = CreateTimeRange(),
                },
                new PatientSchedulingContext
                {
                    DoctorSchedule = CreateSchedule(),
                    InitiatorMembership = CreateSelfMembership(),
                    RequestedCategory = AppointmentCategory.Other,
                },
                null!
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.MissingClearance);
    }

    [Fact]
    public void ScheduleByGuardian_ShouldThrowUnauthorized_WhenInitiatorIsNotGuardian()
    {
        // Arrange
        var args = new GuardianSchedulingArgs
        {
            TargetPatient = CreateMinorPatient(),
            InitiatorUserId = Guid.CreateVersion7(),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
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
    public void ScheduleByGuardian_ShouldThrowUnauthorized_WhenInitiatorIsSelf()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var args = new GuardianSchedulingArgs
        {
            TargetPatient = CreateMinorPatient(),
            InitiatorUserId = userId,
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
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
    public void ScheduleByGuardian_ShouldThrowUnauthorized_WhenPhoneIsNotVerified()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();
        var args = new GuardianSchedulingArgs
        {
            TargetPatient = CreateMinorPatient(),
            InitiatorUserId = authorId,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = false,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
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
    public void ScheduleByGuardian_ShouldThrowIncompleteProfileException_WhenProfileIncomplete()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();
        var incompletePatient = Patient.CreateProfile(
            PersonName.Create("Child"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-10)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new GuardianSchedulingArgs
        {
            TargetPatient = incompletePatient,
            InitiatorUserId = authorId,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
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
        var authorId = Guid.CreateVersion7();
        var target = CreateMinorPatient();
        var args = new GuardianSchedulingArgs
        {
            TargetPatient = target,
            InitiatorUserId = authorId,
            DoctorId = Guid.CreateVersion7(),
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
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
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

        var authorId = Guid.CreateVersion7();
        var args = new GuardianSchedulingArgs
        {
            TargetPatient = CreateMinorPatient(),
            InitiatorUserId = authorId,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
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
    public void ScheduleByGuardian_ShouldThrowDoctorNotAvailableException_WhenNotAvailable()
    {
        // Arrange
        var authorId = Guid.CreateVersion7();
        var args = new GuardianSchedulingArgs
        {
            TargetPatient = CreateMinorPatient(),
            InitiatorUserId = authorId,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = TimeRange.Create(new TimeOnly(18, 0), new TimeOnly(19, 0)),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = CreateSchedule(),
            InitiatorMembership = CreateParentMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
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
        var authorId = Guid.CreateVersion7();
        var patientAdult = Patient.CreateProfile(
            PersonName.Create("Adult"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-25)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patientAdult.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patientAdult.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new GuardianSchedulingArgs
        {
            TargetPatient = patientAdult,
            InitiatorUserId = authorId,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            GuardianNotes = null,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = Schedule.Create(
                args.DoctorId,
                args.ScheduledDate.DayOfWeek,
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
            ),
            InitiatorMembership = CreateParentMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
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
            _fakeTime.GetUtcNow().UtcDateTime
        );
        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var authorId = Guid.CreateVersion7();
        var args = new GuardianSchedulingArgs
        {
            TargetPatient = target,
            InitiatorUserId = authorId,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = scheduledDate,
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            GuardianNotes = null,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = Schedule.Create(
                args.DoctorId,
                args.ScheduledDate.DayOfWeek,
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
            ),
            InitiatorMembership = CreateParentMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByGuardian(
                CreateAppointmentType(),
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
            TargetPatient = target,
            InitiatorUserId = authorId,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            GuardianNotes = "notes",
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = doctorSchedule,
            InitiatorMembership = CreateParentMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByGuardian(
            appointmentType,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.PatientNotes.Should().BeEmpty();
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.AppointmentTypeId.Should().Be(appointmentType.Id);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeRange.Should().Be(args.TimeRange);
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
            TargetPatient = target,
            InitiatorUserId = authorId,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            GuardianNotes = null,
        };

        var context = new PatientSchedulingContext
        {
            DoctorSchedule = Schedule.Create(
                args.DoctorId,
                args.ScheduledDate.DayOfWeek,
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
            ),
            InitiatorMembership = CreateParentMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByGuardian(
            appointmentType,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.PatientNotes.Should().BeEmpty();
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.AppointmentTypeId.Should().Be(appointmentType.Id);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeRange.Should().Be(args.TimeRange);
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
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        return patient;
    }

    private GuardianSchedulingArgs CreateValidGuardianSchedulingArgs() =>
        new()
        {
            TargetPatient = CreateMinorPatient(),
            InitiatorUserId = Guid.CreateVersion7(),
            TimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
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
