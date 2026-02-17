using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;

namespace ClinicFlow.Domain.Entities;

public class Patient : BaseEntity
{
    public Guid UserId { get; init; }
    public DateTime DateOfBirth { get; private set; }
    public string BloodType { get; private set; } = string.Empty;
    public string Allergies { get; private set; } = string.Empty;
    public string ChronicConditions { get; private set; } = string.Empty;
    public string EmergencyContactName { get; private set; } = string.Empty;
    public string EmergencyContactPhone { get; private set; } = string.Empty;

    // EF Core constructor
    private Patient() { }

    private Patient(Guid userId, DateTime dateOfBirth, string bloodType, string allergies, string chronicConditions, string emergencyContactName,
        string emergencyContactPhone) : this()
    {
        UserId = userId;
        DateOfBirth = dateOfBirth;
        BloodType = bloodType;
        ChronicConditions = chronicConditions;
        Allergies = allergies;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
    }

    // Factory Method
    internal static Patient Create(Guid userId, DateTime dateOfBirth, string bloodType, string allergies, string chronicConditions, string emergencyContactName,
        string emergencyContactPhone)
    {
        if (dateOfBirth > DateTime.UtcNow) throw new BusinessRuleValidationException("Date of birth cannot be in the future.");
        if (string.IsNullOrWhiteSpace(emergencyContactName)) throw new BusinessRuleValidationException("Emergency contact name cannot be empty.");
        if (string.IsNullOrWhiteSpace(emergencyContactPhone)) throw new BusinessRuleValidationException("Emergency contact phone cannot be empty.");

        return new Patient(userId, dateOfBirth, bloodType, allergies, chronicConditions, emergencyContactName, emergencyContactPhone);
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