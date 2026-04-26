namespace ClinicFlow.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of an appointment.
/// </summary>
public enum AppointmentStatus
{
    Scheduled = 1,

    InProgress = 3,

    Completed = 4,

    /// <summary>
    /// The appointment was cancelled within the allowed cancellation window.
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// The patient did not attend the appointment.
    /// </summary>
    NoShow = 6,

    /// <summary>
    /// The appointment was cancelled after the allowed cancellation window, incurring a penalty.
    /// </summary>
    LateCancellation = 7,

    /// <summary>
    /// The patient has arrived at the clinic and checked in, waiting for the doctor.
    /// </summary>
    CheckedIn = 8,
}
