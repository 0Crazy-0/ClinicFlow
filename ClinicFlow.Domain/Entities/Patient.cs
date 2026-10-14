using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Patients;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a patient registered in the clinic.
/// Contains medical profile data and booking-eligibility rules.
/// </summary>
public class Patient : BaseEntity
{
    public const int MaxCorrectionReasonLength = 500;

    public PersonName FullName { get; private set; } = null!;

    public DateOnly DateOfBirth { get; private set; }

    public BiologicalSex SexAtBirth { get; private set; }

    public BloodType BloodGroup { get; private set; } = null!;

    // Stryker disable once String
    public string Allergies { get; private set; } = string.Empty;

    // Stryker disable once String
    public string ChronicConditions { get; private set; } = string.Empty;

    public EmergencyContact Contact { get; private set; } = null!;

    // EF Core constructor
    private Patient() { }

    private Patient(PersonName fullName, DateOnly dateOfBirth, BiologicalSex biologicalSex)
        : this()
    {
        FullName = fullName;
        DateOfBirth = dateOfBirth;
        SexAtBirth = biologicalSex;
    }

    internal static Patient CreateProfile(
        PersonName fullName,
        DateOnly dateOfBirth,
        BiologicalSex biologicalSex,
        DateTime referenceTime
    )
    {
        Guard.NotNull(fullName);
        Guard.IsDefined(biologicalSex);

        if (dateOfBirth > DateOnly.FromDateTime(referenceTime))
            throw new DomainValidationException(DomainErrors.Validation.ValueCannotBeInFuture);

        return new Patient(fullName, dateOfBirth, biologicalSex);
    }

    public void UpdateMedicalProfile(
        BloodType bloodType,
        string allergies,
        string chronicConditions
    )
    {
        BloodGroup = bloodType;
        Allergies = allergies ?? string.Empty;
        ChronicConditions = chronicConditions ?? string.Empty;
    }

    /// <summary>
    /// Corrects a birth registration error in the recorded biological sex.
    /// Not a sex change: the original value was wrong and is replaced, keeping an audit event.
    /// </summary>
    public void CorrectBiologicalSex(
        BiologicalSex correctedSex,
        string reason,
        Guid correctedByDoctorId,
        DateTime correctedAt
    )
    {
        Guard.IsDefined(correctedSex);
        Guard.NotNullOrWhiteSpace(reason);
        Guard.NotEmpty(correctedByDoctorId);

        if (reason.Length > MaxCorrectionReasonLength)
            throw new DomainValidationException(DomainErrors.Validation.ValueTooLong);

        if (correctedSex == SexAtBirth)
            throw new BusinessRuleValidationException(DomainErrors.Patient.BiologicalSexUnchanged);

        var previousSex = SexAtBirth;

        SexAtBirth = correctedSex;

        AddDomainEvent(
            new PatientBiologicalSexCorrectedEvent(
                Id,
                previousSex,
                correctedSex,
                correctedByDoctorId,
                reason,
                correctedAt
            )
        );
    }

    public void UpdateEmergencyContact(EmergencyContact emergencyContact) =>
        Contact = emergencyContact;

    internal void EnsureCompleteProfile()
    {
        if (BloodGroup is null || Contact is null)
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
