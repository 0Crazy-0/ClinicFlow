using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Defines the age restrictions and guardian requirements for booking a specific appointment type.
/// </summary>
public record AgeEligibilityPolicy
{
    private const int LegalAdultAge = 18;

    public int? MinimumAge { get; }

    public int? MaximumAge { get; }

    public bool RequiresLegalGuardian { get; }

    public static AgeEligibilityPolicy NoRestriction => new(null, null, false);

    private AgeEligibilityPolicy(int? minimumAge, int? maximumAge, bool requiresLegalGuardian)
    {
        MinimumAge = minimumAge;
        MaximumAge = maximumAge;
        RequiresLegalGuardian = requiresLegalGuardian;
    }

    public static AgeEligibilityPolicy Create(
        int? minimumAge,
        int? maximumAge,
        bool requiresLegalGuardian
    )
    {
        if (minimumAge.HasValue && maximumAge.HasValue && minimumAge.Value > maximumAge.Value)
            throw new DomainValidationException(DomainErrors.AppointmentType.InvalidAgeRange);

        return new AgeEligibilityPolicy(minimumAge, maximumAge, requiresLegalGuardian);
    }

    public void ValidatePatientEligibility(int patientAgeInYears, bool hasGuardianConsent = false)
    {
        if (MinimumAge.HasValue && patientAgeInYears < MinimumAge.Value)
            throw new DomainValidationException(DomainErrors.AppointmentType.MinimumAgeNotMet);

        if (MaximumAge.HasValue && patientAgeInYears > MaximumAge.Value)
            throw new DomainValidationException(DomainErrors.AppointmentType.MaximumAgeExceeded);

        if (RequiresLegalGuardian && patientAgeInYears < LegalAdultAge && !hasGuardianConsent)
            throw new DomainValidationException(DomainErrors.AppointmentType.LegalGuardianRequired);
    }
}
