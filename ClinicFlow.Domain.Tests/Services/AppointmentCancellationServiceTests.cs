using System.Reflection;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Appointments;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Cancellation;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Services;

public class AppointmentCancellationServiceTests
{
    [Fact]
    public void CancelByStaff_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .Build();
        var initiatorUserId = Guid.NewGuid();
        var args = new StaffCancellationArgs(initiatorUserId, CreateSpecialty(24), "Admin Reason");

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
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .Build();
        var initiatorUserId = Guid.NewGuid();
        var args = new StaffCancellationArgs(
            initiatorUserId,
            CreateSpecialty(24),
            "Receptionist Reason"
        );

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
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .Build();
        var args = new StaffCancellationArgs(Guid.NewGuid(), CreateSpecialty(24), reason!);

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
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .Build();
        var doctor = CreateDoctor(appointment.DoctorId, Guid.NewGuid());
        var args = new DoctorCancellationArgs(doctor, CreateSpecialty(24), "Doctor Reason");

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
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .Build();
        var doctor = CreateDoctor(Guid.NewGuid(), Guid.NewGuid());
        var args = new DoctorCancellationArgs(doctor, CreateSpecialty(24), "Doctor reason");

        // Act
        var act = () => AppointmentCancellationService.CancelByDoctor(appointment, args);

        // Assert
        act.Should()
            .Throw<AppointmentCancellationUnauthorizedException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedCancellation);
    }

    [Fact]
    public void CancelByDoctor_ShouldThrowValidationException_WhenDoctorIsMissing()
    {
        // Arrange
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .Build();
        var args = new DoctorCancellationArgs(null, CreateSpecialty(24), "Doctor reason");

        // Act
        var act = () => AppointmentCancellationService.CancelByDoctor(appointment, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
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
        var patient = CreateSelfPatient(patientId, initiatorUserId, age);
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .WithPatientId(patientId)
            .Build();
        var args = new PatientCancellationArgs(
            patient,
            patient,
            category,
            CreateSpecialty(24),
            "Patient reason"
        );

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, args);

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
        var patient = CreateFamilyMemberPatient(patientId, initiatorUserId, relationship, age);
        var initiatorPatient = CreateSelfPatient(Guid.NewGuid(), initiatorUserId, 30);
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .WithPatientId(patientId)
            .Build();
        var args = new PatientCancellationArgs(
            patient,
            initiatorPatient,
            category,
            CreateSpecialty(24),
            "Patient reason"
        );

        // Act
        AppointmentCancellationService.CancelByPatient(appointment, args);

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
        var patient = CreateSelfPatient(patientId, initiatorUserId, 30);
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .WithPatientId(patientId)
            .Build();
        var args = new PatientCancellationArgs(
            patient,
            patient,
            AppointmentCategory.Procedure,
            CreateSpecialty(24),
            "Patient reason"
        );

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args);

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
        var patient = CreateFamilyMemberPatient(patientId, initiatorUserId, relationship, age);
        var initiatorPatient = CreateSelfPatient(Guid.NewGuid(), initiatorUserId, 30);
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .WithPatientId(patientId)
            .Build();
        var args = new PatientCancellationArgs(
            patient,
            initiatorPatient,
            category,
            CreateSpecialty(24),
            "Patient reason"
        );

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args);

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
        var patient = CreateSelfPatient(patientId, anotherUserId, 30);
        var initiatorPatient = CreateSelfPatient(Guid.NewGuid(), initiatorUserId, 30);
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .WithPatientId(patientId)
            .Build();
        var args = new PatientCancellationArgs(
            patient,
            initiatorPatient,
            AppointmentCategory.Checkup,
            CreateSpecialty(24),
            "Patient reason"
        );

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args);

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
        var patient = CreateSelfPatient(patientId, initiatorUserId, 30);
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .Build();
        var args = new PatientCancellationArgs(
            patient,
            patient,
            AppointmentCategory.Checkup,
            CreateSpecialty(24),
            "Patient reason"
        );

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.DataMismatch);
    }

    [Fact]
    public void CancelByPatient_ShouldThrowValidationException_WhenInitiatorHasNoPatientProfile()
    {
        // Arrange
        var initiatorUserId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var patient = CreateSelfPatient(patientId, initiatorUserId, 30);
        var appointment = new AppointmentBuilder()
            .WithScheduledDateTime(DateTime.UtcNow.AddDays(2))
            .WithPatientId(patientId)
            .Build();
        var args = new PatientCancellationArgs(
            patient,
            null,
            AppointmentCategory.Checkup,
            CreateSpecialty(24),
            "Patient reason"
        );

        // Act
        var act = () => AppointmentCancellationService.CancelByPatient(appointment, args);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    private static MedicalSpecialty CreateSpecialty(int minCancellationHours)
    {
        var specialty = (MedicalSpecialty)Activator.CreateInstance(typeof(MedicalSpecialty), true)!;
        SetPrivateProperty(
            specialty,
            nameof(MedicalSpecialty.MinCancellationHours),
            minCancellationHours
        );
        return specialty;
    }

    private class AppointmentBuilder
    {
        private Guid _patientId = Guid.NewGuid();
        private Guid _doctorId = Guid.NewGuid();
        private Guid _typeId = Guid.NewGuid();
        private DateTime _scheduledDateTime = DateTime.UtcNow.AddDays(2);

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

    private static Patient CreateSelfPatient(Guid id, Guid userId, int age)
    {
        var dateOfBirth = DateTime.UtcNow.AddYears(-age);
        var patient = Patient.CreateSelf(userId, PersonName.Create("Test"), dateOfBirth);
        SetPrivateProperty(patient, nameof(Patient.Id), id);
        return patient;
    }

    private static Patient CreateFamilyMemberPatient(
        Guid id,
        Guid userId,
        PatientRelationship relationship,
        int age
    )
    {
        var dateOfBirth = DateTime.UtcNow.AddYears(-age);
        var patient = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Test"),
            relationship,
            dateOfBirth
        );
        SetPrivateProperty(patient, nameof(Patient.Id), id);
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
        SetPrivateProperty(doctor, nameof(Doctor.Id), id);
        return doctor;
    }

    private static void SetPrivateProperty(object obj, string propertyName, object value)
    {
        var type = obj.GetType();

        while (type != null)
        {
            var prop = type.GetProperty(
                propertyName,
                BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Instance
                    | BindingFlags.DeclaredOnly
            );

            if (prop != null)
            {
                prop.SetValue(obj, value);
                return;
            }

            type = type.BaseType;
        }
    }
}
