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

public class ScheduleByStaffTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ScheduleByStaff_ShouldThrowArgumentNullException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                null!,
                CreateValidContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByStaff_ShouldThrowArgumentNullException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldThrowArgumentNullException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldThrowArgumentNullException_WhenClinicTimeZoneIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldThrowArgumentNullException_WhenDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                CreateValidArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByStaff_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldThrowBusinessRuleValidationException_WhenSlotIsAmbiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );
        var ambiguousDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new StaffSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = ambiguousDate,
            RequestedTimeSlot = TimeRange.Create(new TimeOnly(1, 0), new TimeOnly(1, 30)),
        };
        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            ambiguousDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(0, 0), new TimeOnly(23, 59))
        );

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                args,
                new StaffSchedulingContext
                {
                    TargetPatient = target,
                    InitiatorUserId = Guid.CreateVersion7(),
                    DoctorSchedule = doctorSchedule,
                    ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
                    AppointmentType = appointmentType,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Scheduling.AmbiguousTimeSlot);
    }

    [Fact]
    public void ScheduleByStaff_ShouldThrowIncompleteProfileException_WhenProfileIncomplete()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var incompletePatient = Patient.CreateProfile(
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            BiologicalSex.Male,
            Address.Create(
                "123 Main Street",
                AddressState.California,
                "90210",
                Country.UnitedStates
            ),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new StaffSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            HasGuardianConsentVerified = false,
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                args,
                new StaffSchedulingContext
                {
                    TargetPatient = incompletePatient,
                    InitiatorUserId = Guid.CreateVersion7(),
                    DoctorSchedule = doctorSchedule,
                    ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
                    AppointmentType = appointmentType,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<IncompleteProfileException>()
            .WithMessage(DomainErrors.Patient.ProfileIncomplete);
    }

    [Fact]
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenPatientTooYoung()
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

        var args = new StaffSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            HasGuardianConsentVerified = false,
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                args,
                new StaffSchedulingContext
                {
                    TargetPatient = target,
                    InitiatorUserId = Guid.CreateVersion7(),
                    DoctorSchedule = doctorSchedule,
                    ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
                    AppointmentType = appointmentType,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MinimumAgeNotMet);
    }

    [Fact]
    public void ScheduleByStaff_ShouldEnforceAvailability_WhenValid()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new StaffSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(18, 0)),
            HasGuardianConsentVerified = false,
        };

        var scheduleForDifferentDoctor = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                args,
                new StaffSchedulingContext
                {
                    TargetPatient = target,
                    InitiatorUserId = Guid.CreateVersion7(),
                    DoctorSchedule = scheduleForDifferentDoctor,
                    ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
                    AppointmentType = appointmentType,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void ScheduleByStaff_ShouldSucceed_WhenValid()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var initiatorUserId = Guid.CreateVersion7();
        var args = new StaffSchedulingArgs
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            HasGuardianConsentVerified = false,
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByStaff(
            args,
            new StaffSchedulingContext
            {
                TargetPatient = target,
                InitiatorUserId = initiatorUserId,
                DoctorSchedule = doctorSchedule,
                ClinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"),
                AppointmentType = appointmentType,
            },
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(target.Id);
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeSlot.Should().Be(args.RequestedTimeSlot);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.ScheduledByUserId.Should().Be(initiatorUserId);
    }

    private StaffSchedulingArgs CreateValidArgs() =>
        new()
        {
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

    private StaffSchedulingContext CreateValidContext() =>
        new()
        {
            TargetPatient = CreateSelfPatient(),
            AppointmentType = CreateAppointmentType(),
            InitiatorUserId = Guid.CreateVersion7(),
            DoctorSchedule = CreateSchedule(),
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
}
