using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Cancellation;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentCancellationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void CancelByStaff_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .Build();
        var initiatorUserId = Guid.NewGuid();
        var args = new StaffCancellationArgs
        {
            InitiatorUserId = initiatorUserId,
            Reason = "Admin Reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        AppointmentCancellationService.CancelByStaff(appointment, args);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledByUserId.Should().Be(initiatorUserId);
    }

    [Fact]
    public void CancelByStaff_ShouldSucceed_WhenReceptionist()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .Build();
        var initiatorUserId = Guid.NewGuid();
        var args = new StaffCancellationArgs
        {
            InitiatorUserId = initiatorUserId,
            Reason = "Receptionist Reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        AppointmentCancellationService.CancelByStaff(appointment, args);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CancelByStaff_ShouldThrowBusinessRuleValidationException_WhenReasonIsMissing(
        string? reason
    )
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .Build();
        var args = new StaffCancellationArgs
        {
            InitiatorUserId = Guid.NewGuid(),
            Reason = reason!,
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => AppointmentCancellationService.CancelByStaff(appointment, args);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Appointment.MissingCancellationReason);
    }

    [Fact]
    public void CancelByDoctor_ShouldSucceed_WhenDoctorCancelsOwnAppointment()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .Build();
        var doctor = CreateDoctor(appointment.DoctorId, Guid.NewGuid());
        var args = new DoctorCancellationArgs
        {
            InitiatorDoctorId = doctor.Id,
            InitiatorUserId = doctor.UserId,
            Reason = "Doctor Reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        AppointmentCancellationService.CancelByDoctor(appointment, args);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void CancelByDoctor_ShouldThrowUnauthorized_WhenDoctorCancelsOtherDoctorsAppointment()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .Build();
        var doctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid());
        var args = new DoctorCancellationArgs
        {
            InitiatorDoctorId = doctor.Id,
            InitiatorUserId = doctor.UserId,
            Reason = "Doctor reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => AppointmentCancellationService.CancelByDoctor(appointment, args);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedCancellation);
    }

    [Theory]
    [InlineData(30, AppointmentCategory.Checkup)]
    [InlineData(30, AppointmentCategory.Emergency)]
    public void CancelByPatient_ShouldSucceed_WhenPatientIsSelf(
        int age,
        AppointmentCategory category
    )
    {
        // Arrange
        var initiatorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = CreateSelfPatient(
            patientId,
            initiatorUserId,
            age,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .WithPatientId(patientId)
            .Build();

        var context = new AppointmentCancellationContext
        {
            Category = category,
            Specialty = CreateSpecialty(24),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorPatientId = patient.UserId,
            Reason = "Patient reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Should().ContainSingle();
    }

    [Theory]
    [InlineData(PatientRelationship.Child, 10, AppointmentCategory.Checkup)]
    [InlineData(PatientRelationship.Child, 10, AppointmentCategory.Emergency)]
    [InlineData(PatientRelationship.Child, 20, AppointmentCategory.Checkup)]
    [InlineData(PatientRelationship.Spouse, 30, AppointmentCategory.Checkup)]
    [InlineData(PatientRelationship.Parent, 60, AppointmentCategory.FollowUp)]
    public void CancelByPatient_ShouldSucceed_WhenPatientIsFamilyMember(
        PatientRelationship relationship,
        int age,
        AppointmentCategory category
    )
    {
        // Arrange
        var initiatorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = CreateFamilyMemberPatient(
            patientId,
            initiatorUserId,
            relationship,
            age,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var initiatorPatient = CreateSelfPatient(
            Guid.NewGuid(),
            initiatorUserId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .WithPatientId(patientId)
            .Build();

        var context = new AppointmentCancellationContext
        {
            Category = category,
            Specialty = CreateSpecialty(24),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorPatientId = initiatorPatient.UserId,
            Reason = "Patient reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CancelByPatient_ShouldThrowUnauthorized_WhenPatientIsSelfButCategoryIsProcedure()
    {
        // Arrange
        var initiatorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = CreateSelfPatient(
            patientId,
            initiatorUserId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .WithPatientId(patientId)
            .Build();

        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Procedure,
            Specialty = CreateSpecialty(24),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorPatientId = patient.UserId,
            Reason = "Patient reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.CannotCancel);
    }

    [Theory]
    [InlineData(PatientRelationship.Child, 10, AppointmentCategory.Procedure)]
    [InlineData(PatientRelationship.Child, 20, AppointmentCategory.Emergency)]
    [InlineData(PatientRelationship.Spouse, 30, AppointmentCategory.Emergency)]
    [InlineData(PatientRelationship.Spouse, 30, AppointmentCategory.Procedure)]
    [InlineData(PatientRelationship.Parent, 60, AppointmentCategory.Emergency)]
    public void CancelByPatient_ShouldThrowUnauthorized_WhenPatientIsFamilyMemberAndRulesFail(
        PatientRelationship relationship,
        int age,
        AppointmentCategory category
    )
    {
        // Arrange
        var initiatorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = CreateFamilyMemberPatient(
            patientId,
            initiatorUserId,
            relationship,
            age,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var initiatorPatient = CreateSelfPatient(
            Guid.NewGuid(),
            initiatorUserId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .WithPatientId(patientId)
            .Build();
        var context = new AppointmentCancellationContext
        {
            Category = category,
            Specialty = CreateSpecialty(24),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorPatientId = initiatorPatient.UserId,
            Reason = "Patient reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.CannotCancel);
    }

    [Fact]
    public void CancelByPatient_ShouldThrowUnauthorized_WhenPatientDoesNotBelongToUser()
    {
        // Arrange
        var initiatorUserId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = CreateSelfPatient(
            patientId,
            anotherUserId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var initiatorPatient = CreateSelfPatient(
            Guid.NewGuid(),
            initiatorUserId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .WithPatientId(patientId)
            .Build();

        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Checkup,
            Specialty = CreateSpecialty(24),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorPatientId = initiatorPatient.UserId,
            Reason = "Patient reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedCancellation);
    }

    [Fact]
    public void CancelByPatient_ShouldThrowValidationException_WhenDataMismatch()
    {
        // Arrange
        var initiatorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = CreateSelfPatient(
            patientId,
            initiatorUserId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2))
            .Build();

        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Checkup,
            Specialty = CreateSpecialty(24),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorPatientId = patient.UserId,
            Reason = "Patient reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.DataMismatch);
    }

    [Fact]
    public void CancelByPatient_ShouldSetLateCancellation_WhenNoticePeriodIsInsufficient()
    {
        // Arrange
        var initiatorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = CreateSelfPatient(
            patientId,
            initiatorUserId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddHours(2))
            .WithPatientId(patientId)
            .Build();

        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Checkup,
            Specialty = CreateSpecialty(24),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorPatientId = patient.UserId,
            Reason = "Too late",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.LateCancellation);
        appointment.DomainEvents.OfType<AppointmentLateCancelledEvent>().Should().ContainSingle();
    }

    [Theory]
    [InlineData(24, 25, AppointmentStatus.Cancelled)]
    [InlineData(24, 23, AppointmentStatus.LateCancellation)]
    [InlineData(12, 13, AppointmentStatus.Cancelled)]
    [InlineData(12, 11, AppointmentStatus.LateCancellation)]
    [InlineData(2, 3, AppointmentStatus.Cancelled)]
    [InlineData(2, 1, AppointmentStatus.LateCancellation)]
    public void CancelByPatient_ShouldEnforceMinimumHoursPolicy(
        int minHours,
        int hoursUntilAppointment,
        AppointmentStatus expectedStatus
    )
    {
        // Arrange
        var initiatorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = CreateSelfPatient(
            patientId,
            initiatorUserId,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(
                _fakeTime.GetUtcNow().UtcDateTime.AddHours(hoursUntilAppointment)
            )
            .WithPatientId(patientId)
            .Build();

        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Checkup,
            Specialty = CreateSpecialty(minHours),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorPatientId = patient.UserId,
            Reason = "Test Policy",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        appointment.Status.Should().Be(expectedStatus);
    }

    private static MedicalSpecialty CreateSpecialty(int minCancellationHours)
    {
        return MedicalSpecialty.Create(
            "Test Specialty",
            "Test Description",
            30,
            minCancellationHours
        );
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

        public Appointment Build() =>
            Appointment.Schedule(
                _patientId,
                _doctorId,
                _typeId,
                _scheduledDateTime.Date,
                TimeRange.Create(
                    _scheduledDateTime.TimeOfDay,
                    _scheduledDateTime.TimeOfDay.Add(TimeSpan.FromHours(1))
                )
            );
    }

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
        return patient;
    }

    private static Patient CreateFamilyMemberPatient(
        Guid id,
        Guid userId,
        PatientRelationship relationship,
        int age,
        DateTime referenceTime
    )
    {
        var dateOfBirth = referenceTime.AddYears(-age);
        var patient = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Test"),
            relationship,
            dateOfBirth,
            referenceTime
        );
        patient.SetId(id);
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
