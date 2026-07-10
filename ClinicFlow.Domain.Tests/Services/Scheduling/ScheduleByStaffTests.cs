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
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services.Scheduling;

public class ScheduleByStaffTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                null!,
                CreateValidStaffSchedulingArgs(),
                CreateSchedule(),
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenDoctorScheduleIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                CreateAppointmentType(),
                CreateValidStaffSchedulingArgs(),
                null!,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                CreateAppointmentType(),
                CreateValidStaffSchedulingArgs() with
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
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                CreateAppointmentType(),
                CreateValidStaffSchedulingArgs() with
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
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                CreateAppointmentType(),
                CreateValidStaffSchedulingArgs(),
                CreateSchedule(),
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByStaff_ShouldThrowIncompleteProfileException_WhenProfileIncomplete()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var incompletePatient = Patient.CreateSelf(
            Guid.CreateVersion7(),
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new StaffSchedulingArgs
        {
            TargetPatient = incompletePatient,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                appointmentType,
                args,
                doctorSchedule,
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
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, null, false)
        );

        var target = Patient.CreateSelf(
            Guid.CreateVersion7(),
            PersonName.Create("Test"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-15)),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        target.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        target.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new StaffSchedulingArgs
        {
            TargetPatient = target,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new StaffSchedulingArgs
        {
            TargetPatient = target,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsOverbook = true,
            HasGuardianConsentVerified = false,
        };

        var doctorSchedule = CreateSchedule();

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByStaff(
            appointmentType,
            args,
            doctorSchedule,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment.DomainEvents.OfType<AppointmentScheduledEvent>().Should().ContainSingle();
        appointment.Should().NotBeNull();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void ScheduleByStaff_ShouldEnforceAvailability_WhenNotOverbook()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new StaffSchedulingArgs
        {
            TargetPatient = target,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(18, 0)),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var scheduleForDifferentDoctor = CreateSchedule();

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldSucceed_WhenValid()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient();
        var args = new StaffSchedulingArgs
        {
            TargetPatient = target,
            DoctorId = Guid.CreateVersion7(),
            ScheduledDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            TimeRange = CreateTimeRange(),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var doctorSchedule = Schedule.Create(
            args.DoctorId,
            args.ScheduledDate.DayOfWeek,
            TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0))
        );

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByStaff(
            appointmentType,
            args,
            doctorSchedule,
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

    private StaffSchedulingArgs CreateValidStaffSchedulingArgs() =>
        new() { TargetPatient = CreateSelfPatient(), TimeRange = CreateTimeRange() };

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
}
