using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Application.AppointmentTypes.Commands.Shared;

/// <summary>
/// Defines the common structure for commands that manage appointment type definitions.
/// </summary>
public interface IAppointmentTypeCommand
{
    /// <summary>
    /// Gets the category of the appointment (e.g., consultation, procedure).
    /// </summary>
    AppointmentCategory Category { get; }

    /// <summary>
    /// Gets the unique name of the appointment type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description detailing the purpose of the appointment type.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the duration of the appointment type represented as a time span.
    /// </summary>
    TimeSpan DurationMinutes { get; }
}
