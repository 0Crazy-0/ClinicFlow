using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
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

public class RescheduleByGuardianTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void RescheduleByGuardian_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                null!,
                CreateValidGuardianReschedulingArgs(),
                new PatientReschedulingContext
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
    public void RescheduleByGuardian_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                null!,
                new PatientReschedulingContext
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
    public void RescheduleByGuardian_ShouldThrowArgumentNullException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidGuardianReschedulingArgs() with
                {
                    TargetPatient = null!,
                },
                new PatientReschedulingContext
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
    public void RescheduleByGuardian_ShouldThrowArgumentNullException_WhenInitiatorMembershipIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidGuardianReschedulingArgs() with
                {
                    InitiatorMembership = null!,
                },
                new PatientReschedulingContext
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
    public void RescheduleByGuardian_ShouldThrowArgumentNullException_WhenNewTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidGuardianReschedulingArgs() with
                {
                    NewTimeRange = null!,
                },
                new PatientReschedulingContext
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
    public void RescheduleByGuardian_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidGuardianReschedulingArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RescheduleByGuardian_ShouldThrowArgumentNullException_WhenContextDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidGuardianReschedulingArgs(),
                new PatientReschedulingContext
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
    public void RescheduleByGuardian_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                CreateAppointment(Guid.CreateVersion7()),
                CreateValidGuardianReschedulingArgs(),
                new PatientReschedulingContext
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
            .WithMessage(DomainErrors.Reschedule.MissingClearance);
    }

    [Fact]
    public void RescheduleByGuardian_ShouldThrowValidationException_WhenTargetMismatch()
    {
        // Arrange
        var appointment = CreateAppointment(Guid.CreateVersion7());
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(authorId),
            InitiatorUserId = authorId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
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
    public void RescheduleByGuardian_ShouldThrowUnauthorized_WhenInitiatorIsNotGuardian()
    {
        // Arrange
        var target = CreateMinorPatient();
        var appointment = CreateAppointment(target.Id);
        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = CreateSelfMembership(),
            InitiatorUserId = Guid.CreateVersion7(),
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
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
    public void RescheduleByGuardian_ShouldThrowUnauthorized_WhenInitiatorIsSelf()
    {
        // Arrange
        var target = CreateMinorPatient();
        var userId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);
        var originalDate = appointment.ScheduledDate;

        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = FamilyMembership.CreateSelf(
                Guid.CreateVersion7(),
                userId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            InitiatorUserId = userId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        appointment.ScheduledDate.Should().Be(originalDate);
    }

    [Fact]
    public void RescheduleByGuardian_ShouldThrowValidation_WhenPatientIsAdult()
    {
        // Arrange
        var patientAdult = Patient.CreateProfile(
            PersonName.Create("Adult"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-25)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patientAdult.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patientAdult.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567867"));

        var authorId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            patientAdult.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            authorId
        );

        appointment.ClearDomainEvents();
        var originalDate = appointment.ScheduledDate;

        var args = new GuardianReschedulingArgs
        {
            TargetPatient = patientAdult,
            InitiatorMembership = CreateParentMembership(authorId),
            InitiatorUserId = authorId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            NewGuardianNotes = null,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.GuardianRequiresMinor);

        appointment.ScheduledDate.Should().Be(originalDate);
    }

    [Fact]
    public void RescheduleByGuardian_ShouldThrowValidation_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var newDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3));
        var target = Patient.CreateProfile(
            PersonName.Create("Child"),
            newDate.AddYears(-FamilyMembership.MinimumAdultAge),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var authorId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            target.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            authorId
        );
        appointment.ClearDomainEvents();
        var originalDate = appointment.ScheduledDate;

        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(authorId),
            InitiatorUserId = authorId,
            NewDate = newDate,
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            NewGuardianNotes = null,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.GuardianRequiresMinor);

        appointment.ScheduledDate.Should().Be(originalDate);
    }

    [Fact]
    public void RescheduleByGuardian_ShouldThrowUnauthorized_WhenNonAuthorGuardianEditsNotes()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var otherGuardianId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            target.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            authorId
        );

        appointment.ClearDomainEvents();
        appointment.UpdateGuardianNotes("original", authorId);

        var originalDate = appointment.ScheduledDate;

        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(otherGuardianId),
            InitiatorUserId = otherGuardianId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            NewGuardianNotes = "new notes",
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);

        appointment.ScheduledDate.Should().Be(originalDate);
        appointment.GuardianNotes.Should().Be("original");
    }

    [Fact]
    public void RescheduleByGuardian_ShouldThrowUnauthorized_WhenPhoneIsNotVerified()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);
        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(authorId),
            InitiatorUserId = authorId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = false,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
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
    public void RescheduleByGuardian_ShouldThrowPatientBlockedException_WhenHasPenalties()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);
        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(authorId),
            InitiatorUserId = authorId,
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
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
                appointment,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<PatientBlockedException>().WithMessage(DomainErrors.Patient.Blocked);
    }

    [Fact]
    public void RescheduleByGuardian_ShouldThrowDoctorNotAvailableException_WhenNotAvailable()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = CreateAppointment(target.Id);
        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(authorId),
            InitiatorUserId = authorId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = TimeRange.Create(new TimeOnly(18, 0), new TimeOnly(19, 0)),
            IsInitiatorPhoneVerified = true,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByGuardian(
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
    public void RescheduleByGuardian_ShouldUpdateGuardianNotes_WhenValid()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            target.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            authorId
        );
        appointment.ClearDomainEvents();

        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(authorId),
            InitiatorUserId = authorId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            NewGuardianNotes = "new notes",
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        AppointmentReschedulingService.RescheduleByGuardian(
            appointment,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.GuardianNotes.Should().Be(args.NewGuardianNotes);
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
        appointment.PatientNotes.Should().BeEmpty();
    }

    [Fact]
    public void RescheduleByGuardian_ShouldPreserveGuardianNotes_WhenNewGuardianNotesIsNull()
    {
        // Arrange
        var target = CreateMinorPatient();
        var authorId = Guid.CreateVersion7();
        var appointment = Appointment.Schedule(
            target.Id,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)),
            authorId
        );
        appointment.ClearDomainEvents();
        appointment.UpdateGuardianNotes("original", authorId);

        var args = new GuardianReschedulingArgs
        {
            TargetPatient = target,
            InitiatorMembership = CreateParentMembership(authorId),
            InitiatorUserId = authorId,
            NewDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(3)),
            NewTimeRange = CreateTimeRange(),
            IsInitiatorPhoneVerified = true,
            NewGuardianNotes = null,
        };

        var context = new PatientReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        // Act
        AppointmentReschedulingService.RescheduleByGuardian(
            appointment,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.GuardianNotes.Should().Be("original");
        appointment.GuardianNotesAuthorUserId.Should().Be(authorId);
        appointment.PatientNotes.Should().BeEmpty();
    }

    private FamilyMembership CreateParentMembership(Guid userId) =>
        FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            userId,
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

    private GuardianReschedulingArgs CreateValidGuardianReschedulingArgs() =>
        new()
        {
            TargetPatient = CreateMinorPatient(),
            InitiatorMembership = CreateParentMembership(Guid.CreateVersion7()),
            InitiatorUserId = Guid.CreateVersion7(),
            NewTimeRange = CreateTimeRange(),
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

    private static Schedule CreateSchedule(Guid doctorId, DayOfWeek dayOfWeek) =>
        Schedule.Create(
            doctorId,
            dayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );
}
