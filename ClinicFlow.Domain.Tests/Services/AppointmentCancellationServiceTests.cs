using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Cancellation;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.Tests.Shared;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentCancellationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void CancelByStaff_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByStaff(
                null!,
                new StaffCancellationArgs
                {
                    InitiatorUserId = Guid.CreateVersion7(),
                    Reason = "Valid reason",
                    CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
                }
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void CancelByStaff_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var appointment = CreateAppointment();
        var initiatorUserId = Guid.CreateVersion7();
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
        var appointment = CreateAppointment();
        var initiatorUserId = Guid.CreateVersion7();
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
        var appointment = CreateAppointment();
        var args = new StaffCancellationArgs
        {
            InitiatorUserId = Guid.CreateVersion7(),
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
    public void CancelByDoctor_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByDoctor(
                null!,
                new DoctorCancellationArgs
                {
                    InitiatorDoctorId = Guid.CreateVersion7(),
                    InitiatorUserId = Guid.CreateVersion7(),
                    Reason = "Valid reason",
                    CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
                }
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void CancelByDoctor_ShouldSucceed_WhenDoctorCancelsOwnAppointment()
    {
        // Arrange
        var appointment = CreateAppointment();
        var doctor = Doctor.Create(
            appointment.DoctorId,
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.CreateVersion7(),
            "555-0000",
            ConsultationRoom.Create(1, "Room A", 1)
        );

        doctor.SetId(appointment.DoctorId);

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
        var appointment = CreateAppointment();
        var otherDoctorId = Guid.CreateVersion7();
        var otherDoctor = Doctor.Create(
            otherDoctorId,
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.CreateVersion7(),
            "555-0000",
            ConsultationRoom.Create(1, "Room A", 1)
        );

        otherDoctor.SetId(otherDoctorId);

        var args = new DoctorCancellationArgs
        {
            InitiatorDoctorId = otherDoctor.Id, // different from appointment.DoctorId, triggers Unauthorized
            InitiatorUserId = otherDoctor.UserId,
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

    [Fact]
    public void CancelByPatient_ShouldThrowDomainValidationException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                null!,
                CreateValidCancellationContext(),
                CreateValidPatientCancellationArgs()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void CancelByPatient_ShouldThrowDomainValidationException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                CreateAppointment(),
                CreateValidCancellationContext(),
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void CancelByPatient_ShouldThrowDomainValidationException_WhenTargetPatientIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                CreateAppointment(),
                CreateValidCancellationContext(),
                CreateValidPatientCancellationArgs() with
                {
                    TargetPatient = null!,
                }
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void CancelByPatient_ShouldThrowDomainValidationException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                CreateAppointment(),
                null!,
                CreateValidPatientCancellationArgs()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void CancelByPatient_ShouldThrowDomainValidationException_WhenContextSpecialtyIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                CreateAppointment(),
                CreateValidCancellationContext() with
                {
                    Specialty = null!,
                },
                CreateValidPatientCancellationArgs()
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
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
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreateSelfPatient(patientId, initiatorUserId, age);
        var appointment = CreateAppointment(patientId);
        var context = new AppointmentCancellationContext
        {
            Category = category,
            Specialty = CreateSpecialty(),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = patient.UserId,
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
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreateFamilyMemberPatient(patientId, initiatorUserId, relationship, age);
        var initiatorPatient = CreateSelfPatient(Guid.CreateVersion7(), initiatorUserId, 30);
        var appointment = CreateAppointment(patientId);
        var context = new AppointmentCancellationContext
        {
            Category = category,
            Specialty = CreateSpecialty(),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorPatient.UserId,
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
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreateSelfPatient(patientId, initiatorUserId, 30);
        var appointment = CreateAppointment(patientId);

        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Procedure,
            Specialty = CreateSpecialty(),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = patient.UserId,
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
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreateFamilyMemberPatient(patientId, initiatorUserId, relationship, age);
        var initiatorPatient = CreateSelfPatient(Guid.CreateVersion7(), initiatorUserId, 30);
        var appointment = CreateAppointment(patientId);

        var context = new AppointmentCancellationContext
        {
            Category = category,
            Specialty = CreateSpecialty(),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorPatient.UserId,
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
        var initiatorUserId = Guid.CreateVersion7();
        var anotherUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreateSelfPatient(patientId, anotherUserId, 30);
        var initiatorPatient = CreateSelfPatient(Guid.CreateVersion7(), initiatorUserId, 30);
        var appointment = CreateAppointment(patientId);

        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Checkup,
            Specialty = CreateSpecialty(),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorPatient.UserId,
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
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreateSelfPatient(patientId, initiatorUserId, 30);
        var appointment = CreateAppointment();
        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Checkup,
            Specialty = CreateSpecialty(),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = patient.UserId,
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
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreateSelfPatient(patientId, initiatorUserId, 30);
        var scheduledDateTime = _fakeTime.GetUtcNow().UtcDateTime.AddHours(2);
        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(scheduledDateTime),
            TimeRange.Create(
                TimeOnly.FromDateTime(scheduledDateTime),
                TimeOnly.FromDateTime(scheduledDateTime).AddMinutes(30)
            )
        );

        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Checkup,
            Specialty = CreateSpecialty(),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = patient.UserId,
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
    [InlineData(48, 49, AppointmentStatus.Cancelled)]
    [InlineData(48, 47, AppointmentStatus.LateCancellation)]
    public void CancelByPatient_ShouldEnforceMinimumHoursPolicy(
        int minHours,
        int hoursUntilAppointment,
        AppointmentStatus expectedStatus
    )
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreateSelfPatient(patientId, initiatorUserId, 30);
        var scheduledDateTime = _fakeTime.GetUtcNow().UtcDateTime.AddHours(hoursUntilAppointment);
        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(scheduledDateTime),
            TimeRange.Create(
                TimeOnly.FromDateTime(scheduledDateTime),
                TimeOnly.FromDateTime(scheduledDateTime).AddMinutes(30)
            )
        );

        var context = new AppointmentCancellationContext
        {
            Category = AppointmentCategory.Checkup,
            Specialty = MedicalSpecialty.Create("Test Specialty", "Test Description", 30, minHours),
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = patient.UserId,
            Reason = "Test Policy",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        appointment.Status.Should().Be(expectedStatus);
    }

    private static MedicalSpecialty CreateSpecialty() =>
        MedicalSpecialty.Create("Test Specialty", "Test Description", 30, 24);

    private Appointment CreateAppointment() =>
        Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10))
        );

    private Appointment CreateAppointment(Guid patientId) =>
        Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(
                TimeOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
                TimeOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)).AddMinutes(30)
            )
        );

    private Patient CreateSelfPatient(Guid id, Guid userId, int age)
    {
        var dateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-age));
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("Test"),
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.SetId(id);
        return patient;
    }

    private Patient CreateFamilyMemberPatient(
        Guid id,
        Guid userId,
        PatientRelationship relationship,
        int age
    )
    {
        var dateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-age));
        var patient = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Test"),
            relationship,
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        patient.SetId(id);
        return patient;
    }

    private PatientCancellationArgs CreateValidPatientCancellationArgs()
    {
        var patient = CreateSelfPatient(Guid.CreateVersion7(), Guid.CreateVersion7(), 30);

        return new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = patient.UserId,
            Reason = "Valid reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };
    }

    private static AppointmentCancellationContext CreateValidCancellationContext() =>
        new() { Category = AppointmentCategory.Checkup, Specialty = CreateSpecialty() };
}
