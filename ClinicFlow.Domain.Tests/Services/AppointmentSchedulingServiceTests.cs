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

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentSchedulingServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                null!,
                CreateValidPatientSchedulingArgs(),
                new AppointmentSchedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                new AppointmentTypeBuilder().Build(),
                null!,
                new AppointmentSchedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                new AppointmentTypeBuilder().Build(),
                CreateValidPatientSchedulingArgs() with
                {
                    TargetPatient = null!,
                },
                new AppointmentSchedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenInitiatorPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                new AppointmentTypeBuilder().Build(),
                CreateValidPatientSchedulingArgs() with
                {
                    InitiatorPatient = null!,
                },
                new AppointmentSchedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenTimeRangeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(
                new AppointmentTypeBuilder().Build(),
                CreateValidPatientSchedulingArgs() with
                {
                    TimeRange = null!,
                },
                new AppointmentSchedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowUnauthorized_WhenUserIdMismatches()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();
        var initiator = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowUnauthorized_WhenNonSelfSchedulesForDifferentPatient()
    {
        // Arrange — family member tries to schedule for a different patient → should be unauthorized
        var userId = Guid.NewGuid();
        var appointmentType = new AppointmentTypeBuilder().Build();

        var initiator = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Child"),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-10),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        initiator.SetId(Guid.NewGuid());

        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void ScheduleByPatient_ShouldSucceed_WhenNonSelfSchedulesForThemselves()
    {
        // Arrange — family member schedules for themselves → should be allowed
        var userId = Guid.NewGuid();
        var appointmentType = new AppointmentTypeBuilder().Build();

        var familyMember = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Spouse"),
            PatientRelationship.Spouse,
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        familyMember.SetId(Guid.NewGuid());
        familyMember.UpdateMedicalProfile(BloodType.Create("A+"), "", "");
        familyMember.UpdateEmergencyContact(EmergencyContact.Create("Name", "1234567890"));

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = familyMember,
            TargetPatient = familyMember,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            appointmentType,
            args,
            context
        );

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentScheduledEvent>();

        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(familyMember.Id);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowIncompleteProfileException_WhenProfileIncomplete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var appointmentType = new AppointmentTypeBuilder().Build();

        var incompletePatient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        incompletePatient.SetId(Guid.NewGuid());

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = incompletePatient,
            TargetPatient = incompletePatient,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<IncompleteProfileException>()
            .WithMessage(DomainErrors.Patient.ProfileIncomplete);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowPatientBlockedException_WhenHasPenalties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var appointmentType = new AppointmentTypeBuilder().Build();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
        };

        var penalties = new[]
        {
            PatientPenalty.CreateAutomaticBlock(
                target.Id,
                "Reason",
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(5).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        var context = new AppointmentSchedulingContext
        {
            Penalties = penalties,
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(appointmentType, args, context);

        // Assert
        act.Should().Throw<PatientBlockedException>().WithMessage(DomainErrors.Patient.Blocked);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDomainValidationException_WhenTooYoung()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var appointmentType = new AppointmentTypeBuilder().WithAgePolicy(18, null, false).Build();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            15,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MinimumAgeNotMet);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowDoctorNotAvailableException_WhenNotAvailable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var appointmentType = new AppointmentTypeBuilder().Build();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(18, 19), // 6pm - 7pm
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17), // 9am - 5pm
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void ScheduleByPatient_ShouldThrowAppointmentConflictException_WhenConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var appointmentType = new AppointmentTypeBuilder().Build();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11), // 10am - 11am
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17), // 9am - 5pm
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByPatient(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void ScheduleByPatient_ShouldSucceed_WhenAllConditionsMet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var appointmentType = new AppointmentTypeBuilder().Build();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientSchedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            DoctorId = Guid.NewGuid(),
            ScheduledDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
            TimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentSchedulingContext
        {
            DoctorSchedule = CreateSchedule(args.DoctorId, args.ScheduledDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var appointment = AppointmentSchedulingService.ScheduleByPatient(
            appointmentType,
            args,
            context
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

    [Fact]
    public void ScheduleByDoctor_ShouldThrowDomainValidationException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(
                null!,
                CreateValidDoctorSchedulingArgs(),
                new AppointmentSchedulingContext()
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
                new AppointmentTypeBuilder().Build(),
                null!,
                new AppointmentSchedulingContext()
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
                new AppointmentTypeBuilder().Build(),
                CreateValidDoctorSchedulingArgs() with
                {
                    InitiatorDoctor = null!,
                },
                new AppointmentSchedulingContext()
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
                new AppointmentTypeBuilder().Build(),
                CreateValidDoctorSchedulingArgs() with
                {
                    TargetPatient = null!,
                },
                new AppointmentSchedulingContext()
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
                new AppointmentTypeBuilder().Build(),
                CreateValidDoctorSchedulingArgs() with
                {
                    TimeRange = null!,
                },
                new AppointmentSchedulingContext()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldThrowUnauthorized_WhenCategoryInvalid()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder()
            .WithCategory(AppointmentCategory.Checkup)
            .Build();
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
        var act = () =>
            AppointmentSchedulingService.ScheduleByDoctor(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder()
            .WithCategory(AppointmentCategory.FollowUp)
            .Build();
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
            context
        );

        // Assert
        appointment.Should().NotBeNull();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldEnforceAvailability_WhenNotOverbook()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder()
            .WithCategory(AppointmentCategory.FollowUp)
            .Build();
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
            AppointmentSchedulingService.ScheduleByDoctor(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldEnforceConflict_WhenNotOverbook()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder()
            .WithCategory(AppointmentCategory.FollowUp)
            .Build();
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
            AppointmentSchedulingService.ScheduleByDoctor(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void ScheduleByDoctor_ShouldSucceed_WhenNotOverbookAndValid()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder()
            .WithCategory(AppointmentCategory.FollowUp)
            .Build();
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
            context
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

    [Fact]
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentSchedulingService.ScheduleByStaff(
                null!,
                CreateValidStaffSchedulingArgs(),
                new AppointmentSchedulingContext()
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
                new AppointmentTypeBuilder().Build(),
                null!,
                new AppointmentSchedulingContext()
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
                new AppointmentTypeBuilder().Build(),
                CreateValidStaffSchedulingArgs() with
                {
                    TargetPatient = null!,
                },
                new AppointmentSchedulingContext()
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
                new AppointmentTypeBuilder().Build(),
                CreateValidStaffSchedulingArgs() with
                {
                    TimeRange = null!,
                },
                new AppointmentSchedulingContext()
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
        var appointmentType = new AppointmentTypeBuilder().Build();

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
            AppointmentSchedulingService.ScheduleByStaff(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<IncompleteProfileException>()
            .WithMessage(DomainErrors.Patient.ProfileIncomplete);
    }

    [Fact]
    public void ScheduleByStaff_ShouldThrowDomainValidationException_WhenPatientTooYoung()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().WithAgePolicy(18, null, false).Build();
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
            AppointmentSchedulingService.ScheduleByStaff(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentType.MinimumAgeNotMet);
    }

    [Fact]
    public void ScheduleByStaff_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder().Build();
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
            context
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
        var appointmentType = new AppointmentTypeBuilder().Build();
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
            AppointmentSchedulingService.ScheduleByStaff(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void ScheduleByStaff_ShouldEnforceConflict_WhenNotOverbook()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder()
            .WithCategory(AppointmentCategory.Checkup)
            .Build();
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
            AppointmentSchedulingService.ScheduleByStaff(appointmentType, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void ScheduleByStaff_ShouldSucceed_WhenValid()
    {
        // Arrange
        var appointmentType = new AppointmentTypeBuilder()
            .WithCategory(AppointmentCategory.Checkup)
            .Build();
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
            context
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

    private PatientSchedulingArgs CreateValidPatientSchedulingArgs() =>
        new()
        {
            TargetPatient = CreateSelfPatient(
                Guid.NewGuid(),
                Guid.NewGuid(),
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            InitiatorPatient = CreateSelfPatient(
                Guid.NewGuid(),
                Guid.NewGuid(),
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            TimeRange = CreateTimeRange(10, 11),
        };

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

    private class AppointmentTypeBuilder
    {
        private AppointmentCategory _category = AppointmentCategory.Checkup;
        private readonly string _name = "Checkup";
        private readonly string _description = "Description";
        private readonly TimeSpan _durationMinutes = TimeSpan.FromMinutes(30);
        private AgeEligibilityPolicy? _agePolicy = null;

        public AppointmentTypeBuilder WithAgePolicy(int? min, int? max, bool requiresGuardian)
        {
            _agePolicy = AgeEligibilityPolicy.Create(min, max, requiresGuardian);
            return this;
        }

        public AppointmentTypeBuilder WithCategory(AppointmentCategory category)
        {
            _category = category;
            return this;
        }

        public AppointmentTypeDefinition Build() =>
            AppointmentTypeDefinition.Create(
                _category,
                _name,
                _description,
                _durationMinutes,
                _agePolicy
            );
    }

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
            101
        );
        doctor.SetId(id);
        return doctor;
    }
}
