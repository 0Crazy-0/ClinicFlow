using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Represents the minimum notice period required for penalty-free cancellation of appointments within a medical specialty.
/// </summary>
/// <remarks>
/// Only predefined hour values are allowed to ensure consistent, standardized cancellation policies across specialties.
/// A value of 0 hours indicates a flexible policy with no cancellation restrictions.
/// </remarks>
public record CancellationLimit
{
    public static readonly IReadOnlyCollection<int> AllowedHours = Array.AsReadOnly([
        0,
        12,
        24,
        48,
        72,
    ]);

    public int Hours { get; }

    private CancellationLimit(int hours) => Hours = hours;

    public static CancellationLimit FromHours(int hours)
    {
        if (!AllowedHours.Contains(hours))
            throw new DomainValidationException(
                DomainErrors.MedicalSpecialty.InvalidCancellationLimit
            );

        return new CancellationLimit(hours);
    }

    internal bool IsNoticePeriodMet(DateTime appointmentDateTime, DateTime referenceTime)
    {
        var timeUntilAppointment = (appointmentDateTime - referenceTime).TotalHours;
        return timeUntilAppointment >= Hours;
    }
}
