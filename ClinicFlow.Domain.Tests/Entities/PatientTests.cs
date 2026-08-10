using System.Reflection;
using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Patients;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class PatientTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void CreateProfile_ShouldCreatePatient_WhenValidParameters()
    {
        // Arrange
        var dateOfBirth = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30));
        var bloodType = BloodType.Create("O+");
        var allergies = "Penicillin";
        var chronicConditions = "None";
        var emergencyContact = EmergencyContact.Create("Mom", "555-5555");

        // Act
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            dateOfBirth,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(bloodType, allergies, chronicConditions);
        patient.UpdateEmergencyContact(emergencyContact);

        // Assert
        patient.Should().NotBeNull();
        patient.FullName.Should().Be(PersonName.Create("John Doe"));
        patient.DateOfBirth.Should().Be(dateOfBirth);
        patient.BloodType.Should().Be(bloodType);
        patient.Allergies.Should().Be(allergies);
        patient.ChronicConditions.Should().Be(chronicConditions);
        patient.EmergencyContact.Should().Be(emergencyContact);
    }

    [Fact]
    public void CreateProfile_ShouldThrowException_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateProfile(
                PersonName.Create("John Doe"),
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }

    [Fact]
    public void CreateProfile_ShouldThrowException_WhenFullNameIsNull()
    {
        // Arrange & Act
        var act = () =>
            Patient.CreateProfile(
                null!,
                DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateProfile_ShouldNotThrowException_WhenDateOfBirthIsEqualToReferenceTimeDate()
    {
        // Arrange
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;
        var dateOfBirth = DateOnly.FromDateTime(referenceTime);

        // Act
        var act = () =>
            Patient.CreateProfile(PersonName.Create("John Doe"), dateOfBirth, referenceTime);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LeaveFamilyAccount_ShouldThrowException_WhenInitiatorUserIdDoesNotMatch()
    {
        // Arrange
        var patient = CreateFamilyMember();
        var anotherUserId = Guid.CreateVersion7();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act & Assert
        patient
            .Invoking(p => p.LeaveFamilyAccount(anotherUserId, referenceDate))
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedRemoval);
    }

    [Fact]
    public void LeaveFamilyAccount_ShouldThrowException_WhenPatientIsPrimaryUser()
    {
        // Arrange
        var patient = CreatePatient();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        // Act & Assert
        patient
            .Invoking(p => p.LeaveFamilyAccount(patient.UserId, referenceDate))
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.CannotLeaveOwnAccount);
    }

    [Fact]
    public void LeaveFamilyAccount_ShouldThrowException_WhenPatientIsUnderage()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var patient = CreateFamilyMember(
            userId: userId,
            relationship: PatientRelationship.Child,
            ageYears: 17
        );

        // Act & Assert
        patient
            .Invoking(p => p.LeaveFamilyAccount(userId, referenceDate))
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.UnderageCannotLeaveFamilyAccount);
    }

    [Fact]
    public void LeaveFamilyAccount_ShouldRestoreOriginalUser_WhenAdultPatientHasOriginalUserId()
    {
        // Arrange
        var familyUserId = Guid.CreateVersion7();
        var originalUserId = Guid.CreateVersion7();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var patient = CreateFamilyMember(
            userId: familyUserId,
            relationship: PatientRelationship.Spouse,
            ageYears: 25
        );

        SetOriginalUserId(patient, originalUserId);

        // Act
        patient.LeaveFamilyAccount(familyUserId, referenceDate);

        // Assert
        patient.UserId.Should().Be(originalUserId);
        patient.RelationshipToUser.Should().Be(PatientRelationship.Self);
        patient.OriginalUserId.Should().BeNull();
        patient.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void LeaveFamilyAccount_ShouldEmitDomainEventAndNotChangeState_WhenAdultPatientHasNoOriginalUserId()
    {
        // Arrange
        var familyUserId = Guid.CreateVersion7();
        var referenceDate = DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime);

        var patient = CreateFamilyMember(
            userId: familyUserId,
            relationship: PatientRelationship.Spouse,
            ageYears: 25
        );

        // Act
        patient.LeaveFamilyAccount(familyUserId, referenceDate);

        // Assert
        patient.UserId.Should().Be(familyUserId);
        patient.RelationshipToUser.Should().Be(PatientRelationship.Spouse);
        patient.OriginalUserId.Should().BeNull();
        patient.IsDeleted.Should().BeFalse();
        patient
            .DomainEvents.OfType<PatientRequiresOwnAccountToLeaveFamilyEvent>()
            .Should()
            .ContainSingle(e => e.PatientId == patient.Id);
    }

    [Fact]
    public void RemoveFamilyMember_ShouldMarkAsDeleted_WhenInitiatorIsSelfAndPatientHasNoOriginalUserId()
    {
        // Arrange
        var userId = Guid.CreateVersion7();

        var patient = CreateFamilyMember(
            userId: userId,
            relationship: PatientRelationship.Child,
            ageYears: 10
        );

        // Act
        patient.RemoveFamilyMember(userId, PatientRelationship.Self);

        // Assert
        patient.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void RemoveFamilyMember_ShouldRestoreOriginalUser_WhenInitiatorIsSelfAndPatientHasOriginalUserId()
    {
        // Arrange
        var familyUserId = Guid.CreateVersion7();
        var originalUserId = Guid.CreateVersion7();

        var patient = CreateFamilyMember(
            userId: familyUserId,
            relationship: PatientRelationship.Spouse,
            ageYears: 25
        );

        SetOriginalUserId(patient, originalUserId);

        // Act
        patient.RemoveFamilyMember(familyUserId, PatientRelationship.Self);

        // Assert
        patient.UserId.Should().Be(originalUserId);
        patient.RelationshipToUser.Should().Be(PatientRelationship.Self);
        patient.OriginalUserId.Should().BeNull();
        patient.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void RemoveFamilyMember_ShouldThrowException_WhenInitiatorRelationshipIsNotSelf()
    {
        // Arrange
        var userId = Guid.CreateVersion7();

        var patient = CreateFamilyMember(
            userId: userId,
            relationship: PatientRelationship.Child,
            ageYears: 10
        );

        // Act & Assert
        patient
            .Invoking(p => p.RemoveFamilyMember(userId, PatientRelationship.Child))
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedRemoval);
    }

    [Fact]
    public void RemoveFamilyMember_ShouldThrowException_WhenPatientIsPrimaryUser()
    {
        // Arrange
        var patient = CreatePatient();

        // Act & Assert
        patient
            .Invoking(p => p.RemoveFamilyMember(patient.UserId, PatientRelationship.Self))
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.CannotRemovePrimaryUser);
    }

    [Fact]
    public void RemoveFamilyMember_ShouldThrowException_WhenInitiatorUserIdDoesNotMatch()
    {
        // Arrange
        var patient = CreateFamilyMember();
        var anotherUserId = Guid.CreateVersion7();

        // Act & Assert
        patient
            .Invoking(p => p.RemoveFamilyMember(anotherUserId, PatientRelationship.Self))
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedRemoval);
    }

    [Fact]
    public void CloseAccount_ShouldMarkAsDeleted_WhenPatientIsPrimaryUser()
    {
        // Arrange
        var patient = CreatePatient();

        // Act
        patient.CloseAccount();

        // Assert
        patient.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void CloseAccount_ShouldThrowException_WhenPatientIsNotPrimaryUser()
    {
        // Arrange
        var patient = CreateFamilyMember();

        //Act && Assert
        patient
            .Invoking(p => p.CloseAccount())
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Patient.OnlyPrimaryUserCanCloseAccount);
    }

    [Fact]
    public void ReactivateAsPrimary_ShouldUndoDeletionAndRestoreSelfRelationship_WhenCalled()
    {
        // Arrange
        var patient = CreatePatient();
        patient.CloseAccount();

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
        patient.CloseAccount();
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
        var patient = CreateFamilyMember();
        patient.RemoveFamilyMember(patient.UserId, PatientRelationship.Self);

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
        var patient = CreateFamilyMember();
        patient.RemoveFamilyMember(patient.UserId, PatientRelationship.Self);
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
        var patient = CreateFamilyMember();
        patient.RemoveFamilyMember(patient.UserId, PatientRelationship.Self);

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
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
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
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
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
    public void EnsureCompleteProfile_ShouldThrowIncompleteProfileException_WhenBloodTypeIsNullAndEmergencyContactHasValue()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateEmergencyContact(EmergencyContact.Create("Mom", "555-5555"));

        // Act & Assert
        patient
            .Invoking(p => p.EnsureCompleteProfile())
            .Should()
            .Throw<IncompleteProfileException>()
            .WithMessage(DomainErrors.Patient.ProfileIncomplete);
    }

    [Fact]
    public void EnsureCompleteProfile_ShouldThrowIncompleteProfileException_WhenBloodTypeHasValueAndEmergencyContactIsNull()
    {
        // Arrange
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");

        // Act & Assert
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
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
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
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(referenceTime.AddYears(-yearsAgo)),
            referenceTime
        );

        // Act & Assert
        patient.GetAge(DateOnly.FromDateTime(referenceTime)).Should().Be(yearsAgo);
    }

    [Fact]
    public void GetAge_ShouldReturnOriginalAgeWithoutAddingOne_WhenBirthdayHasNotOccurredInReferenceYear()
    {
        // Arrange
        var dayBeforeBirthday = new DateTimeOffset(2026, 6, 19, 0, 0, 0, TimeSpan.Zero);

        _fakeTime.SetUtcNow(dayBeforeBirthday);

        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            new DateOnly(2000, 6, 20),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var age = patient.GetAge(DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime));

        // Assert
        age.Should().Be(25);
    }

    private Patient CreatePatient()
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("John Doe"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        SetUserId(patient, Guid.CreateVersion7());
        SetRelationshipToUser(patient, PatientRelationship.Self);
        patient.UpdateMedicalProfile(BloodType.Create("O+"), "None", "None");
        patient.UpdateEmergencyContact(EmergencyContact.Create("Mom", "555-5555"));

        return patient;
    }

    private Patient CreateFamilyMember(
        Guid? userId = null,
        PatientRelationship relationship = PatientRelationship.Child,
        int ageYears = 10
    )
    {
        var patient = Patient.CreateProfile(
            PersonName.Create("Family Member"),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-ageYears)),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        SetUserId(patient, userId ?? Guid.CreateVersion7());
        SetRelationshipToUser(patient, relationship);
        return patient;
    }

    private static void SetUserId(Patient patient, Guid userId)
    {
        var property = typeof(Patient).GetProperty(
            nameof(Patient.UserId),
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
        );

        property?.SetValue(patient, userId);
    }

    private static void SetRelationshipToUser(Patient patient, PatientRelationship relationship)
    {
        var property = typeof(Patient).GetProperty(
            nameof(Patient.RelationshipToUser),
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
        );

        property?.SetValue(patient, relationship);
    }

    private static void SetOriginalUserId(Patient patient, Guid originalUserId)
    {
        var property = typeof(Patient).GetProperty(
            nameof(Patient.OriginalUserId),
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
        );

        property?.SetValue(patient, originalUserId);
    }
}
