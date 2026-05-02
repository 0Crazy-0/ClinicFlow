using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events;
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
    public void RemoveFamilyMember_ShouldMarkAsDeleted_WhenPatientIsFamilyMember()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var patient = CreateFamilyMember(userId);

        // Act
        patient.RemoveFamilyMember(userId);

        // Assert
        patient.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void RemoveFamilyMember_ShouldThrowException_WhenPatientIsPrimaryUser()
    {
        // Arrange
        var patient = CreatePatient();

        //Act && Assert
        patient
            .Invoking(p => p.RemoveFamilyMember(patient.UserId))
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.CannotRemovePrimaryUser);
    }

    [Fact]
    public void RemoveFamilyMember_ShouldThrowException_WhenInitiatorIsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var patient = CreateFamilyMember(userId);
        var anotherUserId = Guid.NewGuid();

        //Act && Assert
        patient
            .Invoking(p => p.RemoveFamilyMember(anotherUserId))
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedRemoval);
    }

    [Fact]
    public void CloseAccount_ShouldMarkAsDeleted_WhenPrimaryUserHasNoPendingAppointments()
    {
        // Arrange
        var patient = CreatePatient();

        // Act
        patient.CloseAccount(false);

        // Assert
        patient.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void CloseAccount_ShouldThrowException_WhenPatientIsNotPrimaryUser()
    {
        // Arrange
        var patient = CreateFamilyMember(Guid.NewGuid());

        // Act
        var act = () => patient.CloseAccount(false);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.OnlyPrimaryUserCanCloseAccount);
    }

    [Fact]
    public void CloseAccount_ShouldThrowException_WhenPrimaryUserHasPendingAppointments()
    {
        // Arrange
        var patient = CreatePatient();

        // Act
        var act = () => patient.CloseAccount(true);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.CannotCloseAccountWithPendingAppointments);
    }

    [Fact]
    public void ReactivateAsPrimary_ShouldUndoDeletionAndRestoreSelfRelationship_WhenCalled()
    {
        // Arrange
        var patient = CreatePatient();
        patient.CloseAccount(false);

        // Act
        patient.ReactivateAsPrimary();

        // Assert
        patient.IsDeleted.Should().BeFalse();
        patient.RelationshipToUser.Should().Be(PatientRelationship.Self);
    }

    [Fact]
    public void ReactivateAsPrimary_ShouldEmitPatientReactivatedEvent_WhenCalled()
    {
        // Arrange
        var patient = CreatePatient();
        patient.CloseAccount(false);
        patient.ClearDomainEvents();

        // Act
        patient.ReactivateAsPrimary();

        // Assert
        patient.DomainEvents.OfType<PatientReactivatedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void ReactivateAsFamilyMember_ShouldUndoDeletionAndUpdateRelationship_WhenCalled()
    {
        // Arrange
        var patient = CreateFamilyMember(Guid.NewGuid());
        patient.RemoveFamilyMember(patient.UserId);

        // Act
        patient.ReactivateAsFamilyMember(PatientRelationship.Sibling);

        // Assert
        patient.IsDeleted.Should().BeFalse();
        patient.RelationshipToUser.Should().Be(PatientRelationship.Sibling);
    }

    [Fact]
    public void ReactivateAsFamilyMember_ShouldEmitPatientReactivatedEvent_WhenCalled()
    {
        // Arrange
        var patient = CreateFamilyMember(Guid.NewGuid());
        patient.RemoveFamilyMember(patient.UserId);
        patient.ClearDomainEvents();

        // Act
        patient.ReactivateAsFamilyMember(PatientRelationship.Sibling);

        // Assert
        patient.DomainEvents.OfType<PatientReactivatedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void ReactivateAsFamilyMember_ShouldThrowException_WhenRelationshipIsSelf()
    {
        // Arrange
        var patient = CreateFamilyMember(Guid.NewGuid());
        patient.RemoveFamilyMember(patient.UserId);

        // Act
        var act = () => patient.ReactivateAsFamilyMember(PatientRelationship.Self);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.CannotBeSelf);
    }

    [Fact]
    public void EnsureCompleteProfile_ShouldNotThrow_WhenProfileIsComplete()
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
        patient.Invoking(p => p.EnsureCompleteProfile()).Should().NotThrow();
    }

    [Fact]
    public void EnsureCompleteProfile_ShouldThrowIncompleteProfileException_WhenProfileIsIncomplete()
    {
        // Arrange
        var patient = Patient.CreateSelf(
            Guid.NewGuid(),
            PersonName.Create("John Doe"),
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act && Assert
        patient
            .Invoking(p => p.EnsureCompleteProfile())
            .Should()
            .Throw<IncompleteProfileException>()
            .WithMessage(DomainErrors.Patient.ProfileIncomplete);
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
        // Arrange & Act
        var act = () => Patient.EnsureNotBlocked([], _fakeTime.GetUtcNow().UtcDateTime);

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
            PatientPenalty.CreateAutomaticWarning(patient.Id, Guid.NewGuid(), "Warning 1"),
            PatientPenalty.CreateAutomaticWarning(patient.Id, Guid.NewGuid(), "Warning 2"),
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
            PatientPenalty.CreateAutomaticBlock(
                patient.Id,
                "Old Block",
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-1).Date,
                _fakeTime.GetUtcNow().UtcDateTime.AddDays(-2).Date
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
            PatientPenalty.CreateAutomaticBlock(
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

    [Fact]
    public void EnsureNotBlocked_ShouldNotThrow_WhenActiveBlockIsRemoved()
    {
        // Arrange
        var patient = CreatePatient();
        var penalty = PatientPenalty.CreateAutomaticBlock(
            patient.Id,
            "Removed Block",
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(10).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        penalty.Remove();

        var penalties = new List<PatientPenalty> { penalty };

        // Act
        var act = () => Patient.EnsureNotBlocked(penalties, _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should().NotThrow();
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

    private Patient CreateFamilyMember(Guid userId) =>
        Patient.CreateFamilyMember(
            userId,
            PersonName.Create("Family Member"),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-10).Date,
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
