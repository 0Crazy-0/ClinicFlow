namespace ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;

/// <summary>
/// Defines the common structure for commands that reschedule an existing appointment.
/// </summary>
public interface IRescheduleCommand
{
    /// <summary>
    /// Gets the unique identifier of the user initiating the rescheduling request.
    /// </summary>
    Guid InitiatorUserId { get; }

    Guid AppointmentId { get; }

    DateTime NewDate { get; }

    TimeSpan NewStartTime { get; }

    TimeSpan NewEndTime { get; }
}
