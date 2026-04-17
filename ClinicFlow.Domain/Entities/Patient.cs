using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a patient registered in the clinic, linked to a user account.
/// Contains medical profile data and booking-eligibility rules.
/// </summary>
public class Patient : BaseEntity
{
    public Guid UserId { get; init; }

    public PersonName FullName { get; private set; } = null!;

    public PatientRelationship RelationshipToUser { get; private set; }

    public DateTime DateOfBirth { get; private set; }

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
        DateTime dateOfBirth
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
    /// <exception cref="DomainValidationException">Thrown when the user ID is empty or the date of birth is in the future.</exception>
    public static Patient CreateSelf(
        Guid userId,
        PersonName fullName,
        DateTime dateOfBirth,
        DateTime referenceTime
    )
    {
        if (userId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (dateOfBirth > referenceTime)
            throw new DomainValidationException(DomainErrors.Validation.ValueCannotBeInFuture);

        return new Patient(userId, fullName, PatientRelationship.Self, dateOfBirth);
    }

    /// <summary>
    /// Creates a new patient entity representing a family member dependent of a primary user.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the relationship is Self, user ID is empty, or the date of birth is in the future.</exception>
    public static Patient CreateFamilyMember(
        Guid userId,
        PersonName fullName,
        PatientRelationship relationshipToUser,
        DateTime dateOfBirth,
        DateTime referenceTime
    )
    {
        if (relationshipToUser is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.Patient.CannotBeSelf);
        if (userId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (dateOfBirth > referenceTime)
            throw new DomainValidationException(DomainErrors.Validation.ValueCannotBeInFuture);

        return new Patient(userId, fullName, relationshipToUser, dateOfBirth);
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

    /// <summary>
    /// Checks if the patient has all required medical and emergency contact information.
    /// </summary>
    public bool HasCompleteMedicalProfile() =>
        BloodType is not null && EmergencyContact is not null;

    /// <summary>
    /// Ensures the patient's medical profile is complete before allowing certain actions.
    /// </summary>
    /// <exception cref="IncompleteProfileException">Thrown when the profile is incomplete.</exception>
    internal void EnsureCompleteProfile()
    {
        if (!HasCompleteMedicalProfile())
            throw new IncompleteProfileException(DomainErrors.Patient.ProfileIncomplete);
    }

    /// <summary>
    /// Calculates the patient's current age in full years.
    /// </summary>
    public int GetAge(DateTime referenceTime)
    {
        var today = referenceTime.Date;
        var age = today.Year - DateOfBirth.Year;

        if (DateOfBirth.AddYears(age) > today)
            age--;

        return age;
    }

    private static bool IsBlockedFromBooking(
        IEnumerable<PatientPenalty> penalties,
        DateTime referenceTime,
        out DateTime? blockedUntil
    )
    {
        var activePenalties = penalties
            .Where(p =>
                !p.IsRemoved
                && p.Type is PenaltyType.TemporaryBlock
                && p.BlockedUntil.HasValue
                && p.BlockedUntil > referenceTime
            )
            .ToList();

        blockedUntil = activePenalties.Count > 0 ? activePenalties.Max(p => p.BlockedUntil) : null;

        return activePenalties.Count > 0;
    }

    /// <summary>
    /// Verifies that the patient is not currently blocked from booking appointments.
    /// </summary>
    /// <param name="penalties">The patient's existing penalty records.</param>
    /// <exception cref="PatientBlockedException">Thrown when the patient has an active temporary block.</exception>
    internal static void EnsureNotBlocked(
        IEnumerable<PatientPenalty> penalties,
        DateTime referenceTime
    )
    {
        if (IsBlockedFromBooking(penalties, referenceTime, out var blockedUntil))
            throw new PatientBlockedException(
                DomainErrors.Patient.Blocked,
                blockedUntil ?? referenceTime
            );
    }
}
