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

    /// <remarks>
    /// Reference value for statistics and reporting, and a suggested default when
    /// defining new appointment types. Does not drive scheduling directly — the
    /// operative duration is <see cref="AppointmentTypeDefinition.Duration"/>.
    /// </remarks>
    public EncounterDuration TypicalDuration { get; private set; }

    public CancellationLimit CancellationPolicy { get; private set; }

    // EF Core constructor
    private MedicalSpecialty()
    {
        TypicalDuration = null!;
        CancellationPolicy = null!;
    }

    private MedicalSpecialty(
        string name,
        string description,
        EncounterDuration typicalDuration,
        CancellationLimit cancellationPolicy
    )
    {
        Name = name;
        Description = description;
        TypicalDuration = typicalDuration;
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

        var typicalDuration = EncounterDuration.FromMinutes(typicalDurationMinutes);
        var cancellationPolicy = CancellationLimit.FromHours(minCancellationHours);

        return new MedicalSpecialty(name, description, typicalDuration, cancellationPolicy);
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

        var typicalDuration = EncounterDuration.FromMinutes(typicalDurationMinutes);
        var cancellationPolicy = CancellationLimit.FromHours(minCancellationHours);

        Name = name;
        Description = description;
        TypicalDuration = typicalDuration;
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
