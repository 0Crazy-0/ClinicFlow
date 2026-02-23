using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Defines a type of appointment with its name, description, and expected duration.
/// </summary>
public class AppointmentTypeDefinition : BaseEntity
{
    /// <summary>
    /// Enumerated appointment type category.
    /// </summary>
    public AppointmentType Type { get; private set; }

    /// <summary>
    /// Display name of the appointment type.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Description of what this appointment type entails.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Expected duration of the appointment.
    /// </summary>
    public TimeSpan DurationMinutes { get; private set; }

    // EF Core constructor
    private AppointmentTypeDefinition() { }

    private AppointmentTypeDefinition(AppointmentType type, string name, string description, TimeSpan durationMinutes)
    {
        Type = type;
        Name = name;
        Description = description;
        DurationMinutes = durationMinutes;
    }

    /// <summary>
    /// Creates a new appointment type definition.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the name is empty or the duration is not positive.</exception>
    internal static AppointmentTypeDefinition Create(AppointmentType type, string name, string description, TimeSpan durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainValidationException("Appointment type name cannot be empty.");
        if (durationMinutes <= TimeSpan.Zero) throw new DomainValidationException("Duration must be positive.");

        return new AppointmentTypeDefinition(type, name, description, durationMinutes);
    }
}
