using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Patients;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a patient registered in the clinic, linked to a user account.
/// Contains medical profile data and booking-eligibility rules.
/// </summary>
public class Patient : SoftDeletableEntity
{
    private const int MinimumAgeForFamilyAccountAutonomy = 18;

    public Guid UserId { get; private set; }

    /// <summary>
    /// Stores the original user identifier associated with this patient before joining a family account.
    /// </summary>
    /// <remarks>
    /// Null when the patient was created directly as a family member without an independent account.
    /// Used during account removal or departure to restore original primary account ownership.
    /// </remarks>
    public Guid? OriginalUserId { get; private set; }

    public PersonName FullName { get; private set; } = null!;

    public PatientRelationship RelationshipToUser { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public BloodType BloodType { get; private set; } = null!;

    public string Allergies { get; private set; } = string.Empty;

    public string ChronicConditions { get; private set; } = string.Empty;

    public EmergencyContact EmergencyContact { get; private set; } = null!;

    // EF Core constructor
    private Patient() { }

    private Patient(
        Guid userId,
        PersonName fullName,
        PatientRelationship relationshipToUser,
        DateOnly dateOfBirth
    )
        : this()
    {
        UserId = userId;
        FullName = fullName;
        RelationshipToUser = relationshipToUser;
        DateOfBirth = dateOfBirth;
    }

    /// <summary>
    /// Creates a new patient entity for the primary user of an account.
    /// </summary>
    internal static Patient CreateSelf(
        Guid userId,
        PersonName fullName,
        DateOnly dateOfBirth,
        DateTime referenceTime
    )
    {
        if (userId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (fullName is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (dateOfBirth > DateOnly.FromDateTime(referenceTime))
            throw new DomainValidationException(DomainErrors.Validation.ValueCannotBeInFuture);

        return new Patient(userId, fullName, PatientRelationship.Self, dateOfBirth);
    }

    /// <summary>
    /// Creates a new patient entity representing a family member dependent of a primary user.
    /// </summary>
    internal static Patient CreateFamilyMember(
        Guid userId,
        PersonName fullName,
        PatientRelationship relationshipToUser,
        DateOnly dateOfBirth,
        DateTime referenceTime
    )
    {
        if (relationshipToUser is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.Patient.CannotBeSelf);
        if (userId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (fullName is null)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (dateOfBirth > DateOnly.FromDateTime(referenceTime))
            throw new DomainValidationException(DomainErrors.Validation.ValueCannotBeInFuture);

        return new Patient(userId, fullName, relationshipToUser, dateOfBirth);
    }

    public void LeaveFamilyAccount(Guid initiatorUserId, DateOnly referenceDate)
    {
        if (UserId != initiatorUserId)
            throw new DomainValidationException(DomainErrors.Patient.UnauthorizedRemoval);

        if (RelationshipToUser is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.Patient.CannotLeaveOwnAccount);

        if (GetAge(referenceDate) < MinimumAgeForFamilyAccountAutonomy)
            throw new DomainValidationException(
                DomainErrors.Patient.UnderageCannotLeaveFamilyAccount
            );

        if (OriginalUserId is not null)
        {
            UserId = OriginalUserId.Value;
            RelationshipToUser = PatientRelationship.Self;
            OriginalUserId = null;
            return;
        }

        AddDomainEvent(new PatientRequiresOwnAccountToLeaveFamilyEvent(Id));
    }

    public void RemoveFamilyMember(Guid initiatorUserId, PatientRelationship initiatorRelationship)
    {
        if (UserId != initiatorUserId)
            throw new DomainValidationException(DomainErrors.Patient.UnauthorizedRemoval);

        if (initiatorRelationship is not PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.Patient.UnauthorizedRemoval);

        if (RelationshipToUser is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.Patient.CannotRemovePrimaryUser);

        if (OriginalUserId is not null)
        {
            UserId = OriginalUserId.Value;
            RelationshipToUser = PatientRelationship.Self;
            OriginalUserId = null;
            return;
        }

        MarkAsDeleted();
    }

    public void CloseAccount()
    {
        if (RelationshipToUser is not PatientRelationship.Self)
            throw new DomainValidationException(
                DomainErrors.Patient.OnlyPrimaryUserCanCloseAccount
            );

        MarkAsDeleted();
    }

    internal void ReactivateAsPrimary()
    {
        UndoDeletion();

        RelationshipToUser = PatientRelationship.Self;
        AddDomainEvent(new PatientReactivatedEvent(Id));
    }

    internal void ReactivateAsFamilyMember(PatientRelationship newRelationship)
    {
        if (newRelationship is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.Patient.CannotBeSelf);

        UndoDeletion();

        RelationshipToUser = newRelationship;
        AddDomainEvent(new PatientReactivatedEvent(Id));
    }

    public void UpdateMedicalProfile(
        BloodType bloodType,
        string allergies,
        string chronicConditions
    )
    {
        BloodType = bloodType;
        Allergies = allergies ?? string.Empty;
        ChronicConditions = chronicConditions ?? string.Empty;
    }

    public void UpdateEmergencyContact(EmergencyContact emergencyContact) =>
        EmergencyContact = emergencyContact;

    internal void EnsureCompleteProfile()
    {
        if (BloodType is null || EmergencyContact is null)
            throw new IncompleteProfileException(DomainErrors.Patient.ProfileIncomplete);
    }

    public int GetAge(DateOnly referenceDate)
    {
        var age = referenceDate.Year - DateOfBirth.Year;

        if (DateOfBirth.AddYears(age) > referenceDate)
            age--;

        return age;
    }
}
