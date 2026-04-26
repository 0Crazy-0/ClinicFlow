using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

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

    public int MinCancellationHours { get; private set; }

    // EF Core
    private MedicalSpecialty() { }

    private MedicalSpecialty(
        string name,
        string description,
        int typicalDurationMinutes,
        int minCancellationHours
    )
    {
        Name = name;
        Description = description;
        TypicalDurationMinutes = typicalDurationMinutes;
        MinCancellationHours = minCancellationHours;
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
        if (typicalDurationMinutes <= 0)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBePositive);
        if (minCancellationHours < 0)
            throw new DomainValidationException(DomainErrors.Validation.ValueCannotBeNegative);

        return new MedicalSpecialty(
            name,
            description,
            typicalDurationMinutes,
            minCancellationHours
        );
    }

    internal bool IsCancellationAllowed(DateTime appointmentDateTime, DateTime referenceTime)
    {
        var hoursUntilAppointment = (appointmentDateTime - referenceTime).TotalHours;
        return hoursUntilAppointment >= MinCancellationHours;
    }
}
