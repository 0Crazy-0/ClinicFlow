namespace ClinicFlow.Application.Appointments.Commands.Shared.Schedule;

/// <summary>
/// Defines the common structure for commands that schedule a new appointment.
/// </summary>
public interface IScheduleCommand
{
    /// <summary>
    /// Gets the unique identifier of the user initiating the scheduling.
    /// </summary>
    Guid InitiatorUserId { get; }

    /// <summary>
    /// Gets the unique identifier of the patient for whom the appointment is being scheduled.
    /// </summary>
    Guid TargetPatientId { get; }

    Guid AppointmentTypeId { get; }

    DateTime ScheduledDate { get; }

    TimeSpan StartTime { get; }

    TimeSpan EndTime { get; }

    string? PatientNotes { get; }
}
