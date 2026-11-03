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
    public const int MaxDuplicateOverrideReasonLength = 500;

    public PersonName FullName { get; private set; } = null!;

    // Stryker disable once String: the default only covers EF materialization, every factory path writes the real value.
    public string SearchName { get; private set; } = string.Empty;

    /// <summary>
    /// Records the reason why the receptionist registered a patient who scored as
    /// a probable duplicate, serving as an audit trail for that decision.
    /// </summary>
    /// <remarks>
    /// Required when registration proceeds despite a duplicate score in the high band,
    /// null in any other path.
    /// </remarks>
    public string? DuplicateOverrideReason { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public BiologicalSex SexAtBirth { get; private set; }

    public BloodType BloodGroup { get; private set; } = null!;

    // Stryker disable once String
    public string Allergies { get; private set; } = string.Empty;

    // Stryker disable once String
    public string ChronicConditions { get; private set; } = string.Empty;

    public EmergencyContact? Contact { get; private set; }

    public Address HomeAddress { get; private set; } = null!;

    public string? SsnLast4Hash { get; private set; }

    // EF Core constructor
    private Patient() { }

    private Patient(
        PersonName fullName,
        DateOnly dateOfBirth,
        BiologicalSex biologicalSex,
        Address homeAddress,
        string? duplicateOverrideReason
    )
        : this()
    {
        FullName = fullName;
        SearchName = fullName.SearchName;
        DateOfBirth = dateOfBirth;
        SexAtBirth = biologicalSex;
        HomeAddress = homeAddress;
        DuplicateOverrideReason = duplicateOverrideReason;
    }

    internal static Patient CreateProfile(
        PersonName fullName,
        DateOnly dateOfBirth,
        BiologicalSex biologicalSex,
        Address homeAddress,
        DateTime referenceTime,
        string? duplicateOverrideReason = null
    )
    {
        Guard.NotNull(fullName);
        Guard.NotDefault(dateOfBirth);
        Guard.NotNull(homeAddress);
        Guard.IsDefined(biologicalSex);

        if (biologicalSex is BiologicalSex.Unspecified)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (dateOfBirth > DateOnly.FromDateTime(referenceTime))
            throw new DomainValidationException(DomainErrors.Validation.ValueCannotBeInFuture);

        if (duplicateOverrideReason is not null)
        {
            Guard.NotNullOrWhiteSpace(duplicateOverrideReason);

            if (duplicateOverrideReason.Length > MaxDuplicateOverrideReasonLength)
                throw new DomainValidationException(DomainErrors.Validation.ValueTooLong);
        }

        return new Patient(
            fullName,
            dateOfBirth,
            biologicalSex,
            homeAddress,
            duplicateOverrideReason
        );
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

        if (correctedSex is BiologicalSex.Unspecified)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

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

    public void AssignSsnLast4Hash(string ssnLast4Hash)
    {
        Guard.NotNullOrWhiteSpace(ssnLast4Hash);

        SsnLast4Hash = ssnLast4Hash;
    }

    internal void EnsureCompleteProfile()
    {
        if (BloodGroup is null || Contact is null)
            throw new IncompleteProfileException(DomainErrors.Patient.ProfileIncomplete);
    }

    public int GetAge(DateOnly referenceDate)
    {
        Guard.NotDefault(referenceDate);

        return DomainRules.ComputeAge(DateOfBirth, referenceDate);
    }
}
