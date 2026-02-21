using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

public class Patient : BaseEntity
{
    public Guid UserId { get; init; }
    public DateTime DateOfBirth { get; private set; }
    public BloodType BloodType { get; private set; } = null!;
    public string Allergies { get; private set; } = string.Empty;
    public string ChronicConditions { get; private set; } = string.Empty;
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

    // Factory Method
    internal static Patient Create(Guid userId, DateTime dateOfBirth, BloodType bloodType, string allergies, string chronicConditions, EmergencyContact emergencyContact)
    {
        if (userId == Guid.Empty) throw new DomainValidationException("User ID cannot be empty.");
        if (dateOfBirth > DateTime.UtcNow) throw new DomainValidationException("Date of birth cannot be in the future.");

        return new Patient(userId, dateOfBirth, bloodType, allergies, chronicConditions, emergencyContact);
    }

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

    internal static void EnsureNotBlocked(IEnumerable<PatientPenalty> penalties)
    {
        if (IsBlockedFromBooking(penalties, out var blockedUntil)) throw new PatientBlockedException(blockedUntil ?? DateTime.UtcNow);
    }
}