using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Appointments;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
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
    public void CancelByStaff_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByStaff_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () => AppointmentCancellationService.CancelByStaff(appointment, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByStaff_ShouldThrowArgumentNullException_WhenReasonIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();
        var args = new StaffCancellationArgs
        {
            InitiatorUserId = Guid.CreateVersion7(),
            Reason = null!,
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => AppointmentCancellationService.CancelByStaff(appointment, args);

        // Assert
        act.Should().Throw<ArgumentNullException>();
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
    public void CancelByStaff_ShouldThrowBusinessRuleValidationException_WhenReasonIsMissing(
        string reason
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
    public void CancelByDoctor_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByDoctor_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () => AppointmentCancellationService.CancelByDoctor(appointment, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
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
    public void CancelByPatient_ShouldThrowArgumentNullException_WhenAppointmentIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                null!,
                CreateValidCancellationContext(),
                CreateValidPatientCancellationArgs()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByPatient_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                CreateAppointment(),
                null!,
                CreateValidPatientCancellationArgs()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByPatient_ShouldThrowArgumentNullException_WhenContextSpecialtyIsNull()
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByPatient_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                CreateAppointment(),
                CreateValidCancellationContext(),
                null!
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByPatient_ShouldThrowArgumentNullException_WhenTargetPatientIsNull()
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
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByPatient_ShouldThrowArgumentNullException_WhenInitiatorMembershipIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                CreateAppointment(),
                CreateValidCancellationContext() with
                {
                    InitiatorMembership = null!,
                },
                CreateValidPatientCancellationArgs()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(30, AppointmentPurpose.Checkup)]
    [InlineData(30, AppointmentPurpose.Emergency)]
    public void CancelByPatient_ShouldSucceed_WhenPatientIsSelf(int age, AppointmentPurpose purpose)
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreatePatient(patientId, age);
        var appointment = CreateAppointment(patientId);
        var cancelledAt = _fakeTime.GetUtcNow().UtcDateTime;

        var context = new AppointmentCancellationContext
        {
            Purpose = purpose,
            Specialty = CreateSpecialty(),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorUserId,
            Reason = "Patient reason",
            CancelledAt = cancelledAt,
        };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledByUserId.Should().Be(initiatorUserId);
        appointment.CancelledAt.Should().Be(DateOnly.FromDateTime(cancelledAt));
        appointment.CancellationReason.Should().Be(args.Reason);
        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CancelByPatient_ShouldSucceed_WhenParentCancelsMinorEmergency()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreatePatient(patientId, 10);
        var appointment = CreateAppointment(patientId);
        var cancelledAt = _fakeTime.GetUtcNow().UtcDateTime;

        var context = new AppointmentCancellationContext
        {
            Purpose = AppointmentPurpose.Emergency,
            Specialty = CreateSpecialty(),
            InitiatorMembership = FamilyMembership.CreateFamilyMember(
                patientId,
                initiatorUserId,
                PatientRelationship.Parent,
                LegalAuthorityType.Parent,
                FamilyMembershipAccessLevel.Full,
                10,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RequestedCategory = AppointmentCategory.Other,
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorUserId,
            Reason = "Patient reason",
            CancelledAt = cancelledAt,
        };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancelledByUserId.Should().Be(initiatorUserId);
        appointment.CancelledAt.Should().Be(DateOnly.FromDateTime(cancelledAt));
        appointment.CancellationReason.Should().Be(args.Reason);
        appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Should().ContainSingle();
    }

    [Fact]
    public void CancelByPatient_ShouldThrowUnauthorized_WhenPatientIsSelfButCategoryIsProcedure()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreatePatient(patientId, 30);
        var appointment = CreateAppointment(patientId);

        var context = new AppointmentCancellationContext
        {
            Purpose = AppointmentPurpose.Procedure,
            Specialty = CreateSpecialty(),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorUserId,
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
    public void CancelByPatient_ShouldThrowUnauthorized_WhenMembershipDeniesCategory()
    {
        // Arrange
        var patient = CreatePatient(Guid.CreateVersion7(), 30);
        var appointment = CreateAppointment(patient.Id);
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.ViewOnly,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        var context = new AppointmentCancellationContext
        {
            Purpose = AppointmentPurpose.Checkup,
            Specialty = CreateSpecialty(),
            InitiatorMembership = membership,
            RequestedCategory = AppointmentCategory.Other,
        };
        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = Guid.CreateVersion7(),
            Reason = "Patient reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void CancelByPatient_ShouldThrowUnauthorized_WhenOtherMemberCancelsEmergencyAt30()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreatePatient(patientId, 30);
        var appointment = CreateAppointment(patientId);

        var context = new AppointmentCancellationContext
        {
            Purpose = AppointmentPurpose.Emergency,
            Specialty = CreateSpecialty(),
            InitiatorMembership = FamilyMembership.CreateFamilyMember(
                patientId,
                initiatorUserId,
                PatientRelationship.Other,
                LegalAuthorityType.None,
                FamilyMembershipAccessLevel.Full,
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RequestedCategory = AppointmentCategory.Other,
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorUserId,
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
    public void CancelByPatient_ShouldThrowUnauthorized_WhenParentCancelsAdultEmergency()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreatePatient(patientId, 30);
        var appointment = CreateAppointment(patientId);

        var context = new AppointmentCancellationContext
        {
            Purpose = AppointmentPurpose.Emergency,
            Specialty = CreateSpecialty(),

            // Membership created when the patient was a minor and still Active.
            // There is no automatic revocation at adulthood, so the service must revalidate current age.
            InitiatorMembership = FamilyMembership.CreateFamilyMember(
                patientId,
                initiatorUserId,
                PatientRelationship.Parent,
                LegalAuthorityType.Parent,
                FamilyMembershipAccessLevel.Full,
                DomainRules.AdultAge - 1,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RequestedCategory = AppointmentCategory.Other,
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorUserId,
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
    public void CancelByPatient_ShouldThrowUnauthorized_WhenParentCancelsExactlyAdultEmergency()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreatePatient(patientId, DomainRules.AdultAge);
        var appointment = CreateAppointment(patientId);

        var context = new AppointmentCancellationContext
        {
            Purpose = AppointmentPurpose.Emergency,
            Specialty = CreateSpecialty(),

            // Membership created when the patient was a minor and still Active.
            // There is no automatic revocation at adulthood, so the service must revalidate current age.
            InitiatorMembership = FamilyMembership.CreateFamilyMember(
                patientId,
                initiatorUserId,
                PatientRelationship.Parent,
                LegalAuthorityType.Parent,
                FamilyMembershipAccessLevel.Full,
                DomainRules.AdultAge - 1,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
            RequestedCategory = AppointmentCategory.Other,
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorUserId,
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
    public void CancelByPatient_ShouldThrowValidationException_WhenDataMismatch()
    {
        // Arrange
        var initiatorUserId = Guid.CreateVersion7();
        var patientId = Guid.CreateVersion7();
        var patient = CreatePatient(patientId, 30);
        var appointment = CreateAppointment(); // patientId = Guid.CreateVersion7()
        var context = new AppointmentCancellationContext
        {
            Purpose = AppointmentPurpose.Checkup,
            Specialty = CreateSpecialty(),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorUserId,
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
        var patient = CreatePatient(patientId, 30);
        var scheduledDateTime = _fakeTime.GetUtcNow().UtcDateTime.AddHours(2);
        var cancelledAt = _fakeTime.GetUtcNow().UtcDateTime;

        var appointment = Appointment.Schedule(
            patientId,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(scheduledDateTime),
            TimeRange.Create(
                TimeOnly.FromDateTime(scheduledDateTime),
                TimeOnly.FromDateTime(scheduledDateTime).AddMinutes(30)
            ),
            Guid.CreateVersion7()
        );

        var context = new AppointmentCancellationContext
        {
            Purpose = AppointmentPurpose.Checkup,
            Specialty = CreateSpecialty(),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

        var args = new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = initiatorUserId,
            Reason = "Too late",
            CancelledAt = cancelledAt,
        };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, context, args);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.LateCancellation);
        appointment.CancelledByUserId.Should().Be(initiatorUserId);
        appointment.CancelledAt.Should().Be(DateOnly.FromDateTime(cancelledAt));
        appointment.CancellationReason.Should().Be(args.Reason);
        appointment.DomainEvents.OfType<AppointmentLateCancelledEvent>().Should().ContainSingle();
    }

    private static MedicalSpecialty CreateSpecialty() =>
        MedicalSpecialty.Create("Test Specialty", "Test Description", 30, 24);

    private Appointment CreateAppointment() =>
        Appointment.Schedule(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(2)),
            TimeRange.Create(new TimeOnly(9), new TimeOnly(10)),
            Guid.CreateVersion7()
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
            ),
            Guid.CreateVersion7()
        );

    private Patient CreatePatient(Guid id, int age)
    {
        var dateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-age));
        var patient = Patient.CreateProfile(
            PersonName.Create("Test"),
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.SetId(id);
        return patient;
    }

    private PatientCancellationArgs CreateValidPatientCancellationArgs()
    {
        var patient = CreatePatient(Guid.CreateVersion7(), 30);

        return new PatientCancellationArgs
        {
            TargetPatient = patient,
            InitiatorUserId = Guid.CreateVersion7(),
            Reason = "Patient reason",
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };
    }

    private AppointmentCancellationContext CreateValidCancellationContext() =>
        new()
        {
            Purpose = AppointmentPurpose.Checkup,
            Specialty = CreateSpecialty(),
            InitiatorMembership = CreateSelfMembership(),
            RequestedCategory = AppointmentCategory.Other,
        };

    private FamilyMembership CreateSelfMembership() =>
        FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
