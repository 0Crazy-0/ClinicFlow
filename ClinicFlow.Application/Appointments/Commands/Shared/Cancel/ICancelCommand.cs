namespace ClinicFlow.Application.Appointments.Commands.Shared.Cancel;

/// <summary>
/// Defines the common structure for commands that cancel an appointment.
/// </summary>
public interface ICancelCommand
{
    Guid AppointmentId { get; }

    /// <summary>
    /// Gets the unique identifier of the user initiating the cancellation.
    /// </summary>
    Guid InitiatorUserId { get; }

    string? Reason { get; }
}
