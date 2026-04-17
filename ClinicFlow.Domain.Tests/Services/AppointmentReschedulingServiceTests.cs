using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Rescheduling;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentReschedulingServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void RescheduleByPatient_ShouldThrowValidationException_WhenTargetMismatch()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.DataMismatch);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenUserIdMismatches()
    {
        // Arrange
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var initiator = CreateSelfPatient(
            Guid.NewGuid(),
            Guid.NewGuid(),
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithPatientId(target.Id)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowUnauthorized_WhenNonSelfReschedulesForDifferentPatient()
    {
        // same UserId but different Patient.Id → must still throw Unauthorized
        var userId = Guid.NewGuid();

        var initiator = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Child"),
            Enums.PatientRelationship.Child,
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

        var appointment = new AppointmentBuilder()
            .WithPatientId(target.Id)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = initiator,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowPatientBlockedException_WhenHasPenalties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithPatientId(target.Id)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .Build();

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
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

        var context = new AppointmentReschedulingContext
        {
            Penalties = penalties,
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(appointment, args, context);

        // Assert
        act.Should().Throw<PatientBlockedException>().WithMessage(DomainErrors.Patient.Blocked);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowDoctorNotAvailableException_WhenNotAvailable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithPatientId(target.Id)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19), // 6pm - 7pm
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17), // 9am - 5pm
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void RescheduleByPatient_ShouldThrowAppointmentConflictException_WhenConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithPatientId(target.Id)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByPatient(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void RescheduleByPatient_ShouldSucceed_WhenAllConditionsMet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var target = CreateSelfPatient(
            Guid.NewGuid(),
            userId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithPatientId(target.Id)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var args = new PatientReschedulingArgs
        {
            InitiatorPatient = target,
            TargetPatient = target,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        AppointmentReschedulingService.RescheduleByPatient(appointment, args, context);

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentRescheduledEvent>();

        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowUnauthorized_WhenDoctorMismatch()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var invalidDoctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid());
        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = invalidDoctor,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentSchedulingUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedScheduling);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var appointment = new AppointmentBuilder()
            .WithDoctorId(doctorId)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var doctor = CreateDoctor(doctorId, Guid.NewGuid());
        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = true,
        };

        var context = new AppointmentReschedulingContext { HasConflict = true };

        // Act
        AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentRescheduledEvent>();

        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldEnforceAvailability_WhenNotOverbook()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var appointment = new AppointmentBuilder()
            .WithDoctorId(doctorId)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();
        var doctor = CreateDoctor(doctorId, Guid.NewGuid());

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19), // 6pm - 7pm
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(doctorId, args.NewDate.DayOfWeek, 9, 17), // 9am - 5pm
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldThrowConflict_WhenNotOverbookAndHasConflict()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var appointment = new AppointmentBuilder()
            .WithDoctorId(doctorId)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();
        var doctor = CreateDoctor(doctorId, Guid.NewGuid());

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(doctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void RescheduleByDoctor_ShouldSucceed_WhenValid()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var appointment = new AppointmentBuilder()
            .WithDoctorId(doctorId)
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();
        var doctor = CreateDoctor(doctorId, Guid.NewGuid());

        var args = new DoctorReschedulingArgs
        {
            InitiatorDoctor = doctor,
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(doctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        AppointmentReschedulingService.RescheduleByDoctor(appointment, args, context);

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentRescheduledEvent>();

        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    [Fact]
    public void RescheduleByStaff_ShouldBypassAvailability_WhenOverbook()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var args = new StaffReschedulingArgs
        {
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = true,
        };

        var context = new AppointmentReschedulingContext { HasConflict = true };

        // Act
        AppointmentReschedulingService.RescheduleByStaff(appointment, args, context);

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentRescheduledEvent>();

        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    [Fact]
    public void RescheduleByStaff_ShouldEnforceAvailability_WhenNotOverbook()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var args = new StaffReschedulingArgs
        {
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(18, 19),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(appointment, args, context);

        // Assert
        act.Should()
            .Throw<DoctorNotAvailableException>()
            .WithMessage(DomainErrors.Schedule.DoctorNotAvailable);
    }

    [Fact]
    public void RescheduleByStaff_ShouldThrowConflict_WhenNotOverbookAndHasConflict()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var args = new StaffReschedulingArgs
        {
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = true,
        };

        // Act
        var act = () =>
            AppointmentReschedulingService.RescheduleByStaff(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentConflictException>()
            .WithMessage(DomainErrors.Appointment.Conflict);
    }

    [Fact]
    public void RescheduleByStaff_ShouldSucceed_WhenValid()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2).Date)
            .Build();

        var args = new StaffReschedulingArgs
        {
            NewDate = _fakeTime.GetUtcNow().UtcDateTime.AddDays(3).Date,
            NewTimeRange = CreateTimeRange(10, 11),
            IsOverbook = false,
        };

        var context = new AppointmentReschedulingContext
        {
            DoctorSchedule = CreateSchedule(appointment.DoctorId, args.NewDate.DayOfWeek, 9, 17),
            HasConflict = false,
        };

        // Act
        AppointmentReschedulingService.RescheduleByStaff(appointment, args, context);

        // Assert
        appointment
            .DomainEvents.Should()
            .ContainSingle()
            .Which.Should()
            .BeOfType<AppointmentRescheduledEvent>();

        appointment.ScheduledDate.Should().Be(args.NewDate);
        appointment.TimeRange.Should().Be(args.NewTimeRange);
    }

    private static TimeRange CreateTimeRange(int startHour, int endHour) =>
        TimeRange.Create(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour));

    private static Schedule CreateSchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        int startHour,
        int endHour
    ) => Schedule.Create(doctorId, dayOfWeek, CreateTimeRange(startHour, endHour));

    private static Patient CreateSelfPatient(Guid id, Guid userId, int age, DateTime referenceTime)
    {
        var dateOfBirth = referenceTime.AddYears(-age);
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            dateOfBirth,
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

    private class AppointmentBuilder
    {
        private Guid _patientId = Guid.NewGuid();
        private Guid _doctorId = Guid.NewGuid();
        private Guid _typeId = Guid.NewGuid();
        private DateTime _scheduledDateTime;

        public AppointmentBuilder WithPatientId(Guid patientId)
        {
            _patientId = patientId;
            return this;
        }

        public AppointmentBuilder WithDoctorId(Guid doctorId)
        {
            _doctorId = doctorId;
            return this;
        }

        public AppointmentBuilder WithTypeId(Guid typeId)
        {
            _typeId = typeId;
            return this;
        }

        public AppointmentBuilder WithScheduledDateTime(DateTime dt)
        {
            _scheduledDateTime = dt;
            return this;
        }

        public Appointment Build()
        {
            var appointment = Appointment.Schedule(
                _patientId,
                _doctorId,
                _typeId,
                _scheduledDateTime.Date,
                TimeRange.Create(
                    _scheduledDateTime.TimeOfDay,
                    _scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
                )
            );
            // Clear construction events for test isolation
            appointment.ClearDomainEvents();

            return appointment;
        }
    }
}
