using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a medical specialty offered by the clinic, including its cancellation policy.
/// </summary>
public class MedicalSpecialty : BaseEntity
{
    /// <summary>
    /// Display name of the medical specialty.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Description of the medical specialty.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Default consultation duration for this specialty, in minutes.
    /// </summary>
    public int TypicalDurationMinutes { get; private set; }

    /// <summary>
    /// Minimum number of hours before an appointment at which cancellation is still allowed without penalty.
    /// </summary>
    public int MinCancellationHours { get; private set; }

    // EF Core
    private MedicalSpecialty() { }

    private MedicalSpecialty(string name, string description, int typicalDurationMinutes, int minCancellationHours)
    {
        Name = name;
        Description = description;
        TypicalDurationMinutes = typicalDurationMinutes;
        MinCancellationHours = minCancellationHours;
    }

    /// <summary>
    /// Creates a new medical specialty.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the name is empty, the duration is not positive, or cancellation hours are negative.</exception>
    internal static MedicalSpecialty Create(string name, string description, int typicalDurationMinutes, int minCancellationHours)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainValidationException("Specialty name cannot be empty.");
        if (typicalDurationMinutes <= 0) throw new DomainValidationException("Duration must be positive.");
        if (minCancellationHours < 0) throw new DomainValidationException("Cancellation hours cannot be negative.");

        return new MedicalSpecialty(name, description, typicalDurationMinutes, minCancellationHours);
    }

    /// <summary>
    /// Determines whether an appointment can be cancelled without penalty based on
    /// the specialty's minimum cancellation notice requirement.
    /// </summary>
    /// <param name="appointmentDateTime">The UTC date and time of the appointment.</param>
    /// <returns><see langword="true"/> if enough notice remains; otherwise, <see langword="false"/>.</returns>
    public bool IsCancellationAllowed(DateTime appointmentDateTime)
    {
        var hoursUntilAppointment = (appointmentDateTime - DateTime.UtcNow).TotalHours;
        return hoursUntilAppointment >= MinCancellationHours;
    }
}
