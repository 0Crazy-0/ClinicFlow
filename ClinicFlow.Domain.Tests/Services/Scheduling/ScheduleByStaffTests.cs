using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
                new AppointmentSchedulingContext(),
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
                new AppointmentSchedulingContext(),
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
                new AppointmentSchedulingContext(),
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
                new AppointmentSchedulingContext(),
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
                new AppointmentSchedulingContext(),
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
            Guid.NewGuid(),
            PersonName.Create("Test"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        incompletePatient.SetId(Guid.NewGuid());

        var args = new StaffSchedulingArgs
        {
            TargetPatient = incompletePatient,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenPatientTooYoung()
    {
        // Arrange
        var appointmentType = AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, null, false)
        );

        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            15,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new StaffSchedulingArgs
        {
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new StaffSchedulingArgs
        {
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(18, 19),
            IsOverbook = true,
            HasGuardianConsentVerified = false,
        };

        var context = new AppointmentSchedulingContext { HasConflict = true };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByStaff(
            appointmentType,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentScheduledEvent>();

        appointment.Should().NotBeNull();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void ScheduleByStaff_ShouldEnforceAvailability_WhenNotOverbook()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new StaffSchedulingArgs
        {
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(18, 19), // 6pm - 7pm
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17), // 9am - 5pm
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
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
    public void ScheduleByStaff_ShouldEnforceConflict_WhenNotOverbook()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new StaffSchedulingArgs
        {
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                appointmentType,
                args,
                context,
                SchedulingClearance.Granted()
            );

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void ScheduleByStaff_ShouldSucceed_WhenValid()
    {
        // Arrange
        var appointmentType = CreateAppointmentType();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new StaffSchedulingArgs
        {
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByStaff(
            appointmentType,
            args,
            context,
            SchedulingClearance.Granted()
        );

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentScheduledEvent>();

        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(target.Id);
        appointment.DoctorId.Should().Be(args.DoctorId);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeRange.Should().Be(args.TimeRange);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    private StaffSchedulingArgs CreateValidStaffSchedulingArgs() =>
        new()
        {
            TargetPatient = CreateSelfPatient(
                Guid.NewGuid(),
                Guid.NewGuid(),
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            TimeRange = CreateTimeRange(10, 11),
        };

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour));

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        int startHour,
        int endHour
    ) => Schedule.Create(doctorId, dayOfWeek, CreateTimeRange(startHour, endHour));

    private static AppointmentTypeDefinition CreateAppointmentType() =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Checkup,
            "Checkup",
            "Description",
            TimeSpan.FromMinutes(30),
            null
        );

    private static Patient CreateSelfPatient(Guid id, Guid userId, int age, DateTime referenceTime)
    {
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            referenceTime.AddYears(-age),
            referenceTime
        );

        patient.SetId(id);
        patient.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        return patient;
    }
}
