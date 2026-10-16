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
                new StaffCancellationArgs { Reason = "Valid reason" },
                CreateValidStaffContext()
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
        var act = () =>
            AppointmentCancellationService.CancelByStaff(
                appointment,
                null!,
                CreateValidStaffContext()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByStaff_ShouldThrowArgumentNullException_WhenReasonIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();
        var args = new StaffCancellationArgs { Reason = null! };

        // Act
        var act = () =>
            AppointmentCancellationService.CancelByStaff(
                appointment,
                args,
                CreateValidStaffContext()
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByStaff_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();
        var args = new StaffCancellationArgs { Reason = "Valid reason" };

        // Act
        var act = () => AppointmentCancellationService.CancelByStaff(appointment, args, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByStaff_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var appointment = CreateAppointment();
        var initiatorUserId = Guid.CreateVersion7();
        var args = new StaffCancellationArgs { Reason = "Admin Reason" };

        // Act
        AppointmentCancellationService.CancelByStaff(
            appointment,
            args,
            new StaffCancellationContext
            {
                InitiatorUserId = initiatorUserId,
                CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
            }
        );

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
        var args = new StaffCancellationArgs { Reason = "Receptionist Reason" };

        // Act
        AppointmentCancellationService.CancelByStaff(
            appointment,
            args,
            new StaffCancellationContext
            {
                InitiatorUserId = initiatorUserId,
                CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
            }
        );

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
        var args = new StaffCancellationArgs { Reason = reason! };

        // Act
        var act = () =>
            AppointmentCancellationService.CancelByStaff(
                appointment,
                args,
                CreateValidStaffContext()
            );

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
                new DoctorCancellationArgs { Reason = "Doctor reason" },
                new DoctorCancellationContext
                {
                    InitiatorDoctor = CreateDoctor(),
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
        var act = () =>
            AppointmentCancellationService.CancelByDoctor(
                appointment,
                null!,
                new DoctorCancellationContext
                {
                    InitiatorDoctor = CreateDoctor(),
                    CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByDoctor_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () =>
            AppointmentCancellationService.CancelByDoctor(
                appointment,
                new DoctorCancellationArgs { Reason = "Doctor reason" },
                null!
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByDoctor_ShouldThrowArgumentNullException_WhenInitiatorDoctorIsNull()
    {
        // Arrange
        var appointment = CreateAppointment();

        // Act
        var act = () =>
            AppointmentCancellationService.CancelByDoctor(
                appointment,
                new DoctorCancellationArgs { Reason = "Doctor reason" },
                new DoctorCancellationContext
                {
                    InitiatorDoctor = null!,
                    CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
                }
            );

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

        var args = new DoctorCancellationArgs { Reason = "Doctor Reason" };

        // Act
        AppointmentCancellationService.CancelByDoctor(
            appointment,
            args,
            new DoctorCancellationContext
            {
                InitiatorDoctor = doctor,
                CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
            }
        );

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

        var args = new DoctorCancellationArgs { Reason = "Doctor reason" };

        // Act
        var act = () =>
            AppointmentCancellationService.CancelByDoctor(
                appointment,
                args,
                new DoctorCancellationContext
                {
                    InitiatorDoctor = otherDoctor,
                    CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
                }
            );

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
                CreateValidArgs(),
                CreateValidContext()
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
                CreateValidArgs(),
                null!
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
                CreateValidArgs(),
                CreateValidContext() with
                {
                    Specialty = null!,
                }
            );

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CancelByPatient_ShouldThrowArgumentNullException_WhenContextAppointmentTypeIsNull()
    {
        // Arrange & Act
        var act = () =>
            AppointmentCancellationService.CancelByPatient(
                CreateAppointment(),
                CreateValidArgs(),
                CreateValidContext() with
                {
                    AppointmentType = null!,
                }
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
                null!,
                CreateValidContext()
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
                CreateValidArgs(),
                CreateValidContext() with
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
                CreateValidArgs(),
                CreateValidContext() with
                {
                    InitiatorMembership = null!,
                }
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
            TargetPatient = patient,
            AppointmentType = CreateAppointmentType(purpose),
            CancelledAt = cancelledAt,
            Specialty = CreateSpecialty(),
            InitiatorMembership = FamilyMembership.CreateSelf(
                patientId,
                initiatorUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        var args = new PatientCancellationArgs { Reason = "Patient reason" };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, args, context);

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
            TargetPatient = patient,
            AppointmentType = CreateAppointmentType(AppointmentPurpose.Emergency),
            CancelledAt = cancelledAt,
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
        };

        var args = new PatientCancellationArgs { Reason = "Patient reason" };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, args, context);

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
            TargetPatient = patient,
            AppointmentType = CreateAppointmentType(AppointmentPurpose.Procedure),
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
            Specialty = CreateSpecialty(),
            InitiatorMembership = CreateSelfMembership(),
        };

        var args = new PatientCancellationArgs { Reason = "Patient reason" };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args, context);

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
            TargetPatient = patient,
            AppointmentType = CreateAppointmentType(AppointmentPurpose.Checkup),
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
            Specialty = CreateSpecialty(),
            InitiatorMembership = membership,
        };
        var args = new PatientCancellationArgs { Reason = "Patient reason" };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args, context);

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
            TargetPatient = patient,
            AppointmentType = CreateAppointmentType(AppointmentPurpose.Emergency),
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
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
        };

        var args = new PatientCancellationArgs { Reason = "Patient reason" };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args, context);

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
            TargetPatient = patient,
            AppointmentType = CreateAppointmentType(AppointmentPurpose.Emergency),
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
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
        };

        var args = new PatientCancellationArgs { Reason = "Patient reason" };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args, context);

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
            TargetPatient = patient,
            AppointmentType = CreateAppointmentType(AppointmentPurpose.Emergency),
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
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
        };

        var args = new PatientCancellationArgs { Reason = "Patient reason" };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args, context);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.CannotCancel);
    }

    [Fact]
    public void CancelByPatient_ShouldThrowValidationException_WhenDataMismatch()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var patient = CreatePatient(patientId, 30);
        var appointment = CreateAppointment(); // patientId = Guid.CreateVersion7()
        var context = new AppointmentCancellationContext
        {
            TargetPatient = patient,
            AppointmentType = CreateAppointmentType(AppointmentPurpose.Checkup),
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
            Specialty = CreateSpecialty(),
            InitiatorMembership = CreateSelfMembership(),
        };

        var args = new PatientCancellationArgs { Reason = "Patient reason" };

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args, context);

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
            TargetPatient = patient,
            AppointmentType = CreateAppointmentType(AppointmentPurpose.Checkup),
            CancelledAt = cancelledAt,
            Specialty = CreateSpecialty(),
            InitiatorMembership = FamilyMembership.CreateSelf(
                patientId,
                initiatorUserId,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        var args = new PatientCancellationArgs { Reason = "Too late" };

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, args, context);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.LateCancellation);
        appointment.CancelledByUserId.Should().Be(initiatorUserId);
        appointment.CancelledAt.Should().Be(DateOnly.FromDateTime(cancelledAt));
        appointment.CancellationReason.Should().Be(args.Reason);
        appointment.DomainEvents.OfType<AppointmentLateCancelledEvent>().Should().ContainSingle();
    }

    private static AppointmentTypeDefinition CreateAppointmentType(
        AppointmentPurpose purpose = AppointmentPurpose.Checkup
    ) =>
        AppointmentTypeDefinition.Create(
            AppointmentCategory.Other,
            purpose,
            "Checkup",
            "Description",
            EncounterDuration.FromMinutes(30),
            null
        );

    private static Doctor CreateDoctor() =>
        Doctor.Create(
            Guid.CreateVersion7(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.CreateVersion7(),
            "555-0000",
            ConsultationRoom.Create(1, "Room A", 1)
        );

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
            BiologicalSex.Male,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.SetId(id);
        return patient;
    }

    private PatientCancellationArgs CreateValidArgs() => new() { Reason = "Patient reason" };

    private AppointmentCancellationContext CreateValidContext() =>
        new()
        {
            TargetPatient = CreatePatient(Guid.CreateVersion7(), 30),
            AppointmentType = CreateAppointmentType(),
            Specialty = CreateSpecialty(),
            InitiatorMembership = CreateSelfMembership(),
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private StaffCancellationContext CreateValidStaffContext() =>
        new()
        {
            InitiatorUserId = Guid.CreateVersion7(),
            CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
        };

    private FamilyMembership CreateSelfMembership() =>
        FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
