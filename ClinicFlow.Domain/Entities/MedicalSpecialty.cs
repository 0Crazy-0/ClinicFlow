using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a medical specialty offered by the clinic.
/// </summary>
/// <remarks>
/// Cancellation policies and penalty-free windows in this domain are strictly tied to the specialty
/// rather than global clinic rules.
/// </remarks>
public class MedicalSpecialty : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public int TypicalDurationMinutes { get; private set; }

    public CancellationLimit CancellationPolicy { get; private set; }

    // EF Core
    private MedicalSpecialty()
    {
        CancellationPolicy = null!;
    }

    private MedicalSpecialty(
        string name,
        string description,
        int typicalDurationMinutes,
        CancellationLimit cancellationPolicy
    )
    {
        Name = name;
        Description = description;
        TypicalDurationMinutes = typicalDurationMinutes;
        CancellationPolicy = cancellationPolicy;
    }

    public static MedicalSpecialty Create(
        string name,
        string description,
        int typicalDurationMinutes,
        int minCancellationHours
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (typicalDurationMinutes <= 0)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);

        var cancellationPolicy = CancellationLimit.FromHours(minCancellationHours);

        return new MedicalSpecialty(name, description, typicalDurationMinutes, cancellationPolicy);
    }

    public void UpdateDetails(
        string name,
        string description,
        int typicalDurationMinutes,
        int minCancellationHours
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (typicalDurationMinutes <= 0)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);

        var cancellationPolicy = CancellationLimit.FromHours(minCancellationHours);

        Name = name;
        Description = description;
        TypicalDurationMinutes = typicalDurationMinutes;
        CancellationPolicy = cancellationPolicy;
    }

    public void Reactivate()
    {
        if (!IsDeleted)
            throw new BusinessRuleValidationException(DomainErrors.MedicalSpecialty.AlreadyActive);

        UndoDeletion();
    }

    public void Deactivate(bool hasActiveDoctors)
    {
        if (IsDeleted)
            throw new BusinessRuleValidationException(
                DomainErrors.MedicalSpecialty.AlreadyInactive
            );
        if (hasActiveDoctors)
            throw new BusinessRuleValidationException(
                DomainErrors.MedicalSpecialty.HasActiveDoctors
            );

        MarkAsDeleted();
    }

    internal bool IsCancellationAllowed(DateTime appointmentDateTime, DateTime referenceTime) =>
        CancellationPolicy.IsNoticePeriodMet(appointmentDateTime, referenceTime);
}
