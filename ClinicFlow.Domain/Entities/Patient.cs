using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions;

namespace ClinicFlow.Domain.Entities;

public class Patient : BaseEntity
{
    public Guid UserId { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string BloodType { get; private set; } = string.Empty;
    public string Allergies { get; private set; } = string.Empty;
    public string ChronicConditions { get; private set; } = string.Empty;
    public string EmergencyContactName { get; private set; } = string.Empty;
    public string EmergencyContactPhone { get; private set; } = string.Empty;

    // EF Core constructor
    private Patient() { }

    public Patient(Guid userId, DateTime dateOfBirth, string bloodType, string allergies, string chronicConditions, string emergencyContactName, string emergencyContactPhone) : this()
    {
        UserId = userId;
        DateOfBirth = dateOfBirth;
        BloodType = bloodType;
        ChronicConditions = chronicConditions;
        Allergies = allergies;
        EmergencyContactName = emergencyContactName;
        EmergencyContactPhone = emergencyContactPhone;
    }

    public int GetAge()
    {
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Year;

        if (DateOfBirth.AddYears(age) > today) age--;

        return age;
    }

    private bool IsBlockedFromBooking(IEnumerable<PatientPenalty> penalties, out DateTime? blockedUntil)
    {
        var activePenalties = penalties.Where(p => p.PenaltyType is PenaltyTypeEnum.TemporaryBlock && p.BlockedUntil.HasValue && p.BlockedUntil > DateTime.UtcNow).ToList();

        blockedUntil = activePenalties.Any() ? activePenalties.Max(p => p.BlockedUntil) : null;

        return activePenalties.Any();
    }

    public void EnsureNotBlocked(IEnumerable<PatientPenalty> penalties)
    {
        if (IsBlockedFromBooking(penalties, out var blockedUntil))
            throw new PatientBlockedException(blockedUntil ?? DateTime.UtcNow);
    }
}