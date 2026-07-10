using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Scheduling;

public class ScheduleByDoctorTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                null!,
                CreateValidDoctorSchedulingArgs(),
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(),
                null!,
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(),
                CreateValidDoctorSchedulingArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenInitiatorDoctorIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(),
                CreateValidDoctorSchedulingArgs() with
                {
                    InitiatorDoctor = null!,
                },
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(),
                CreateValidDoctorSchedulingArgs() with
                {
                    TargetPatient = null!,
                },
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(),
                CreateValidDoctorSchedulingArgs() with
                {
                    TimeRange = null!,
                },
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(),
                CreateValidDoctorSchedulingArgs(),
                CreateSchedule(),
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenPatientTooYoung()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, null, false)
        );

        var doctor = CreateDoctor();
        var target = Patient.CreateSelf(
            Guid.CreateVersion7(),
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-15)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new DoctorSchedulingArgs
        {
            InitiatorDoctor = doctor,
            TargetPatient = target,
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                appointmentType,
                args,
                doctorSchedule,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MinimumAgeNotMet);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var doctor = CreateDoctor();
        var target = CreateSelfPatient();
        var args = new DoctorSchedulingArgs
        {
            InitiatorDoctor = doctor,
            TargetPatient = target,
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsOverbook = true,
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByDoctor(
            appointmentType,
            args,
            doctorSchedule,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.Should().NotBeNull();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldEnforceAvailability_WhenNotOverbook()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var doctor = CreateDoctor();
        var target = CreateSelfPatient();
        var args = new DoctorSchedulingArgs
        {
            InitiatorDoctor = doctor,
            TargetPatient = target,
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(18, 0)),
            IsOverbook = false,
        };

        var scheduleForDifferentDoctor = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                appointmentType,
                args,
                scheduleForDifferentDoctor,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldSucceed_WhenNotOverbookAndValid()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var doctor = CreateDoctor();
        var target = CreateSelfPatient();
        var args = new DoctorSchedulingArgs
        {
            InitiatorDoctor = doctor,
            TargetPatient = target,
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsOverbook = false,
        };

        var doctorSchedule = Schedule.Create(
            doctor.Id,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByDoctor(
            appointmentType,
            args,
            doctorSchedule,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(target.Id);
        appointment.DoctorId.Should().Be(args.InitiatorDoctor.Id);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeRange.Should().Be(args.TimeRange);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    private DoctorSchedulingArgs CreateValidDoctorSchedulingArgs() =>
        new()
        {
            InitiatorDoctor = CreateDoctor(),
            TargetPatient = CreateSelfPatient(),
            TimeRange = CreateTimeRange(),
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
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            null
        );

    private Patient CreateSelfPatient()
    {
        var patient = Patient.CreateSelf(
            Guid.CreateVersion7(),
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
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
