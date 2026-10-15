using System.Globalization;
using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Scheduling;

public class ScheduleByDoctorTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ScheduleByDoctor_ShouldThrowArgumentNullException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
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
    public void ScheduleByDoctor_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                null!,
                CreateValidContext(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowArgumentNullException_WhenInitiatorDoctorIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateValidArgs(),
                CreateValidContext() with
                {
                    InitiatorDoctor = null!,
                },
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowArgumentNullException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
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
    public void ScheduleByDoctor_ShouldThrowArgumentNullException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
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
    public void ScheduleByDoctor_ShouldThrowArgumentNullException_WhenClinicTimeZoneIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
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
    public void ScheduleByDoctor_ShouldThrowArgumentNullException_WhenDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
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
    public void ScheduleByDoctor_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateValidArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
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
    public void ScheduleByDoctor_ShouldThrowBusinessRuleValidationException_WhenSlotIsAmbiguous()
    {
        // Arrange
        _fakeTime.SetUtcNow(
            DateTimeOffset.Parse("2026-11-01T12:00:00Z", CultureInfo.InvariantCulture)
        );

        var ambiguousDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);
        var appointmentType = CreateAppointmentType();
        var doctor = CreateDoctor();
        var target = CreateSelfPatient();

        var args = new DoctorSchedulingArgs
        {
            ScheduledDate = ambiguousDate,
            RequestedTimeSlot = TimeRange.Create(new TimeOnly(1, 0), new TimeOnly(1, 30)),
        };

        var doctorSchedule = Schedule.Create(
            doctor.Id,
            ambiguousDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(0, 0), new TimeOnly(23, 59))
        );

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                args,
                new DoctorSchedulingContext
                {
                    InitiatorDoctor = doctor,
                    TargetPatient = target,
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
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenPatientTooYoung()
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

        var doctor = CreateDoctor();
        var target = Patient.CreateProfile(
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-15)),
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new DoctorSchedulingArgs
        {
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
            HasGuardianConsentVerified = false,
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                args,
                new DoctorSchedulingContext
                {
                    InitiatorDoctor = doctor,
                    TargetPatient = target,
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
    public void ScheduleByDoctor_ShouldEnforceAvailability_WhenValid()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var doctor = CreateDoctor();
        var target = CreateSelfPatient();
        var args = new DoctorSchedulingArgs
        {
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(18, 0)),
        };

        var scheduleForDifferentDoctor = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                args,
                new DoctorSchedulingContext
                {
                    InitiatorDoctor = doctor,
                    TargetPatient = target,
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
    public void ScheduleByDoctor_ShouldSucceed_WhenValid()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var doctor = CreateDoctor();
        var target = CreateSelfPatient();
        var args = new DoctorSchedulingArgs
        {
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

        var doctorSchedule = Schedule.Create(
            doctor.Id,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByDoctor(
            args,
            new DoctorSchedulingContext
            {
                InitiatorDoctor = doctor,
                TargetPatient = target,
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
        appointment.DoctorId.Should().Be(doctor.Id);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeSlot.Should().Be(args.RequestedTimeSlot);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.ScheduledByUserId.Should().Be(doctor.UserId);
    }

    private DoctorSchedulingArgs CreateValidArgs() =>
        new()
        {
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            RequestedTimeSlot = CreateTimeRange(),
        };

    private DoctorSchedulingContext CreateValidContext() =>
        new()
        {
            InitiatorDoctor = CreateDoctor(),
            TargetPatient = CreateSelfPatient(),
            AppointmentType = CreateAppointmentType(),
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
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        return patient;
    }

    private static Doctor CreateDoctor() =>
        Doctor.Create(
            Guid.CreateVersion7(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.CreateVersion7(),
            "555-0000",
            ConsultationRoom.Create(1, "Room A", 1)
        );
}
