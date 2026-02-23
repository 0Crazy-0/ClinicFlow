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
    /// <summary>
    /// Identifier of the associated user account.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Patient's date of birth.
    /// </summary>
    public DateTime DateOfBirth { get; private set; }

    /// <summary>
    /// Patient's blood type.
    /// </summary>
    public BloodType BloodType { get; private set; } = null!;

    /// <summary>
    /// Known allergies of the patient (free-text).
    /// </summary>
    public string Allergies { get; private set; } = string.Empty;

    /// <summary>
    /// Known chronic conditions of the patient (free-text).
    /// </summary>
    public string ChronicConditions { get; private set; } = string.Empty;

    /// <summary>
    /// Emergency contact information for the patient.
    /// </summary>
    public EmergencyContact EmergencyContact { get; private set; } = null!;

    // EF Core constructor
    private Patient() { }

    private Patient(Guid userId, DateTime dateOfBirth, BloodType bloodType, string allergies, string chronicConditions, EmergencyContact emergencyContact) : this()
    {
        UserId = userId;
        DateOfBirth = dateOfBirth;
        BloodType = bloodType;
        ChronicConditions = chronicConditions;
        Allergies = allergies;
        EmergencyContact = emergencyContact;
    }

    /// <summary>
    /// Creates a new patient entity.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the user ID is empty or the date of birth is in the future.</exception>
    internal static Patient Create(Guid userId, DateTime dateOfBirth, BloodType bloodType, string allergies, string chronicConditions, EmergencyContact emergencyContact)
    {
        if (userId == Guid.Empty) throw new DomainValidationException("User ID cannot be empty.");
        if (dateOfBirth > DateTime.UtcNow) throw new DomainValidationException("Date of birth cannot be in the future.");

        return new Patient(userId, dateOfBirth, bloodType, allergies, chronicConditions, emergencyContact);
    }

    /// <summary>
    /// Calculates the patient's current age in full years.
    /// </summary>
    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;

        if (DateOfBirth.AddYears(age) > today) age--;

        return age;
    }

    private static bool IsBlockedFromBooking(IEnumerable<PatientPenalty> penalties, out DateTime? blockedUntil)
    {
        var activePenalties = penalties.Where(p => p.Type is PenaltyType.TemporaryBlock && p.BlockedUntil.HasValue && p.BlockedUntil > DateTime.UtcNow).ToList();

        blockedUntil = activePenalties.Count > 0 ? activePenalties.Max(p => p.BlockedUntil) : null;

        return activePenalties.Count > 0;
    }

    /// <summary>
    /// Verifies that the patient is not currently blocked from booking appointments.
    /// </summary>
    /// <param name="penalties">The patient's existing penalty records.</param>
    /// <exception cref="PatientBlockedException">Thrown when the patient has an active temporary block.</exception>
    internal static void EnsureNotBlocked(IEnumerable<PatientPenalty> penalties)
    {
        if (IsBlockedFromBooking(penalties, out var blockedUntil)) throw new PatientBlockedException(blockedUntil ?? DateTime.UtcNow);
    }
}