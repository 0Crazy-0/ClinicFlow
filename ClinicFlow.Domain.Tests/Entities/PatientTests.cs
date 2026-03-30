using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class PatientTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Create_ShouldCreatePatient_WhenValidParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dateOfBirth = _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date;
        var bloodType = BloodType.Create("O+");
        var allergies = "Penicillin";
        var chronicConditions = "None";
        var emergencyContact = EmergencyContact.Create("Mom", "555-5555");

        // Act
        var patient = Patient.CreateSelf(
            userId,
            PersonName.Create("John Doe"),
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(bloodType, allergies, chronicConditions);
        patient.UpdateEmergencyContact(emergencyContact);

        // Assert
        patient.Should().NotBeNull();
        patient.UserId.Should().Be(userId);
        patient.DateOfBirth.Should().Be(dateOfBirth);
        patient.BloodType.Should().Be(bloodType);
        patient.Allergies.Should().Be(allergies);
        patient.ChronicConditions.Should().Be(chronicConditions);
        patient.EmergencyContact.Should().Be(emergencyContact);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateSelf(
                Guid.NewGuid(),
                PersonName.Create("John Doe"),
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateSelf(
                Guid.Empty,
                PersonName.Create("John Doe"),
                _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateFamilyMember_ShouldCreatePatient_WhenValidParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dateOfBirth = _fakeTime.GetUtcNow().UtcDateTime.AddYears(-10).Date;

        // Act
        var patient = Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Family Member"),
            PatientRelationship.Child,
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Assert
        patient.Should().NotBeNull();
        patient.UserId.Should().Be(userId);
        patient.RelationshipToUser.Should().Be(PatientRelationship.Child);
        patient.DateOfBirth.Should().Be(dateOfBirth);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenRelationshipIsSelf()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateFamilyMember(
                Guid.NewGuid(),
                PersonName.Create("Family Member"),
                PatientRelationship.Self,
                _fakeTime.GetUtcNow().UtcDateTime.AddYears(-10).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.CannotBeSelf);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateFamilyMember(
                Guid.Empty,
                PersonName.Create("Family Member"),
                PatientRelationship.Child,
                _fakeTime.GetUtcNow().UtcDateTime.AddYears(-10).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateFamilyMember(
                Guid.NewGuid(),
                PersonName.Create("Family Member"),
                PatientRelationship.Child,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(1).Date,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }

    [Fact]
    public void HasCompleteMedicalProfile_ShouldReturnFalse_WhenJustCreated()
    {
        // Arrange
        var patient = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("John Doe"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        patient.HasCompleteMedicalProfile().Should().BeFalse();
    }

    [Fact]
    public void HasCompleteMedicalProfile_ShouldReturnTrue_WhenProfileIsCompleted()
    {
        // Arrange
        var patient = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("John Doe"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Mom", "555-5555"));

        // Act & Assert
        patient.HasCompleteMedicalProfile().Should().BeTrue();
    }

    [Fact]
    public void UpdateMedicalProfile_ShouldSetEmptyString_WhenNullStringsAreProvided()
    {
        // Arrange
        var patient = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("John Doe"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        var bloodType = BloodType.Create("A-");

        // Act
        patient.UpdateMedicalProfile(bloodType, null!, null!);

        // Assert
        patient.BloodType.Should().Be(bloodType);
        patient.Allergies.Should().Be(string.Empty);
        patient.ChronicConditions.Should().Be(string.Empty);
    }

    [Fact]
    public void GetAge_ShouldReturnCorrectAge()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var yearsAgo = 25;
        var patient = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("John Doe"),
            referenceTime.AddYears(-yearsAgo).Date,
            referenceTime
        );

        // Act & Assert
        patient.GetAge(referenceTime).Should().Be(yearsAgo);
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenNoPenalties()
    {
        // Arrange
        var penalties = new List<PatientPenalty>();

        // Act
        var act = () => Patient.EnsureNotBlocked(penalties, _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenOnlyWarnings()
    {
        // Arrange
        var patient = CreatePatient();
        var penalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateWarning(patient.Id, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateWarning(patient.Id, Guid.NewGuid(), "Warning 2"),
        };

        // Act
        var act = () => Patient.EnsureNotBlocked(penalties, _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenBlockExpired()
    {
        // Arrange
        var patient = CreatePatient();
        var penalties = new List<PatientPenalty>
        {
            CreateExpiredBlock(
                patient.Id,
                "Old Block",
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date
            ),
        };

        // Act
        var act = () => Patient.EnsureNotBlocked(penalties, _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNotBlocked_ShouldThrowPatientBlockedException_WhenActiveBlockExists()
    {
        // Arrange
        var patient = CreatePatient();
        var blockedUntil = _fakeTime.GetUtcNow().UtcDateTime.AddDays(10).Date;
        var penalties = new List<PatientPenalty>
        {
            PatientPenalty.CreateBlock(
                patient.Id,
                "Active Block",
                blockedUntil,
                _fakeTime.GetUtcNow().UtcDateTime
            ),
        };

        // Act
        var act = () => Patient.EnsureNotBlocked(penalties, _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<PatientBlockedException>()
            .WithMessage(DomainErrors.Patient.Blocked)
            .Where(e => e.BlockedUntil == blockedUntil);
    }

    private Patient CreatePatient()
    {
        var patient = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("John Doe"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Mom", "555-5555"));
        return patient;
    }

    private static PatientPenalty CreateExpiredBlock(
        Guid patientId,
        string reason,
        DateTime blockedUntil
    )
    {
        var penalty = (PatientPenalty)Activator.CreateInstance(typeof(PatientPenalty), true)!;

        typeof(PatientPenalty)
            .GetProperty(nameof(PatientPenalty.PatientId))!
            .SetValue(penalty, patientId);
        typeof(PatientPenalty)
            .GetProperty(nameof(PatientPenalty.Type))!
            .SetValue(penalty, PenaltyType.TemporaryBlock);
        typeof(PatientPenalty)
            .GetProperty(nameof(PatientPenalty.Reason))!
            .SetValue(penalty, reason);
        typeof(PatientPenalty)
            .GetProperty(nameof(PatientPenalty.BlockedUntil))!
            .SetValue(penalty, blockedUntil);

        return penalty;
    }
}
