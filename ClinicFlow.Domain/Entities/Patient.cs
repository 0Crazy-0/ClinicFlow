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

    // Stryker disable once String: the default only covers EF materialization, every factory path writes the real value.
    public string SearchName { get; private set; } = string.Empty;

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
        Address homeAddress
    )
        : this()
    {
        FullName = fullName;
        SearchName = fullName.SearchName;
        DateOfBirth = dateOfBirth;
        SexAtBirth = biologicalSex;
        HomeAddress = homeAddress;
    }

    internal static Patient CreateProfile(
        PersonName fullName,
        DateOnly dateOfBirth,
        BiologicalSex biologicalSex,
        Address homeAddress,
        DateTime referenceTime
    )
    {
        Guard.NotNull(fullName);
        Guard.NotNull(homeAddress);
        Guard.IsDefined(biologicalSex);

        if (biologicalSex is BiologicalSex.Unspecified)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (dateOfBirth > DateOnly.FromDateTime(referenceTime))
            throw new DomainValidationException(DomainErrors.Validation.ValueCannotBeInFuture);

        return new Patient(fullName, dateOfBirth, biologicalSex, homeAddress);
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
        var age = referenceDate.Year - DateOfBirth.Year;

        if (DateOfBirth.AddYears(age) > referenceDate)
            age--;

        return age;
    }
}
