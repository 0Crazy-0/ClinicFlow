namespace ClinicFlow.Domain.Enums;

/// <summary>
/// Categorizes the clinical purpose of an appointment.
/// </summary>
public enum AppointmentPurpose
{
    /// <summary>
    /// Initial visit with a new patient.
    /// </summary>
    FirstConsultation,

    /// <summary>
    /// Return visit to review a previous diagnosis or treatment.
    /// </summary>
    FollowUp,

    /// <summary>
    /// Urgent, unscheduled visit requiring immediate attention.
    /// </summary>
    Emergency,

    /// <summary>
    /// Routine preventive health examination.
    /// </summary>
    Checkup,

    /// <summary>
    /// Medical or surgical procedure.
    /// </summary>
    Procedure,
}
