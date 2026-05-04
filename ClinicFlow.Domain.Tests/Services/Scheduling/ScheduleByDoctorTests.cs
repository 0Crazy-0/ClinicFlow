using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Scheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
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
                new AppointmentSchedulingContext(),
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
                CreateAppointmentType(AppointmentCategory.Checkup),
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
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenInitiatorDoctorIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(AppointmentCategory.Checkup),
                CreateValidDoctorSchedulingArgs() with
                {
                    InitiatorDoctor = null!,
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
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(AppointmentCategory.Checkup),
                CreateValidDoctorSchedulingArgs() with
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
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(AppointmentCategory.Checkup),
                CreateValidDoctorSchedulingArgs() with
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
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenClearanceIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                CreateAppointmentType(AppointmentCategory.Checkup),
                CreateValidDoctorSchedulingArgs(),
                new AppointmentSchedulingContext(),
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
            TimeSpan.FromMinutes(30),
            AgeEligibilityPolicy.Create(18, null, false)
        );

        var doctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid());
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            15,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new DoctorSchedulingArgs
        {
            InitiatorDoctor = doctor,
            TargetPatient = target,
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
            HasGuardianConsentVerified = false,
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(
                args.InitiatorDoctor.Id,
                args.ScheduledDate.DayOfWeek,
                9,
                17
            ),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
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
    public void ScheduleByDoctor_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var appointmentType = CreateAppointmentType(AppointmentCategory.FollowUp);
        var doctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid());
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new DoctorSchedulingArgs
        {
            InitiatorDoctor = doctor,
            TargetPatient = target,
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(18, 19),
            IsOverbook = true,
        };

        var context = new AppointmentSchedulingContext { HasConflict = true };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByDoctor(
            appointmentType,
            args,
            context,
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
        var appointmentType = CreateAppointmentType(AppointmentCategory.FollowUp);
        var doctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid());
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new DoctorSchedulingArgs
        {
            InitiatorDoctor = doctor,
            TargetPatient = target,
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(18, 19), // 6pm - 7pm
            IsOverbook = false,
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(doctor.Id, args.ScheduledDate.DayOfWeek, 9, 17), // 9am - 5pm
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
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
    public void ScheduleByDoctor_ShouldEnforceConflict_WhenNotOverbook()
    {
        // Arrange
        var appointmentType = CreateAppointmentType(AppointmentCategory.FollowUp);
        var doctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid());
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new DoctorSchedulingArgs
        {
            InitiatorDoctor = doctor,
            TargetPatient = target,
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(doctor.Id, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
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
    public void ScheduleByDoctor_ShouldSucceed_WhenNotOverbookAndValid()
    {
        // Arrange
        var appointmentType = CreateAppointmentType(AppointmentCategory.FollowUp);
        var doctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid());
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new DoctorSchedulingArgs
        {
            InitiatorDoctor = doctor,
            TargetPatient = target,
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(doctor.Id, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByDoctor(
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
        appointment.DoctorId.Should().Be(args.InitiatorDoctor.Id);
        appointment.ScheduledDate.Should().Be(args.ScheduledDate);
        appointment.TimeRange.Should().Be(args.TimeRange);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    private DoctorSchedulingArgs CreateValidDoctorSchedulingArgs() =>
        new()
        {
            InitiatorDoctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid()),
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

    private static AppointmentTypeDefinition CreateAppointmentType(AppointmentCategory category) =>
        AppointmentTypeDefinition.Create(
            category,
            "Checkup",
            "Description",
            TimeSpan.FromMinutes(30),
            null
        );

    private static Patient CreateSelfPatient(Guid id, Guid userId, int age, DateTime referenceTime)
    {
        ;
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

    private static Doctor CreateDoctor(Guid id, Guid userId)
    {
        var doctor = Doctor.Create(
            userId,
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "555-0000",
            ConsultationRoom.Create(101, "Room A", 1)
        );
        doctor.SetId(id);
        return doctor;
    }
}
