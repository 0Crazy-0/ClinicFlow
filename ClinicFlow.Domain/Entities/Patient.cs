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
    public Guid UserId { get; init; }

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
    public static Patient CreateSelf(
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
    public static Patient CreateFamilyMember(
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

    public void RemoveFamilyMember(Guid initiatorUserId)
    {
        if (UserId != initiatorUserId)
            throw new DomainValidationException(DomainErrors.Patient.UnauthorizedRemoval);

        if (RelationshipToUser is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.Patient.CannotRemovePrimaryUser);

        MarkAsDeleted();
    }

    /// <param name="hasPendingAppointments">Indicates if the patient has any active, future appointments.</param>
    public void CloseAccount(bool hasPendingAppointments)
    {
        if (RelationshipToUser is not PatientRelationship.Self)
            throw new DomainValidationException(
                DomainErrors.Patient.OnlyPrimaryUserCanCloseAccount
            );

        if (hasPendingAppointments)
            throw new DomainValidationException(
                DomainErrors.Patient.CannotCloseAccountWithPendingAppointments
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

    internal static void EnsureNotBlocked(
        IReadOnlyList<PatientPenalty> penalties,
        DateOnly referenceDate
    )
    {
        var activePenalties = penalties
            .Where(p =>
                !p.IsRemoved
                && p.Type is PenaltyType.TemporaryBlock
                && p.BlockedUntil.HasValue
                && p.BlockedUntil.Value > referenceDate
            )
            .ToList();

        if (activePenalties.Count > 0)
            throw new PatientBlockedException(
                DomainErrors.Patient.Blocked,
                activePenalties.Max(p => p.BlockedUntil) ?? referenceDate
            );
    }
}
