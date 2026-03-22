namespace ClinicFlow.Domain.Enums;

/// <summary>
/// Categorizes the clinical purpose of an appointment.
/// </summary>
public enum AppointmentCategory
{
    /// <summary>
    /// Initial visit with a new patient.
    /// </summary>
    FirstConsultation = 1,

    /// <summary>
    /// Return visit to review a previous diagnosis or treatment.
    /// </summary>
    FollowUp = 2,

    /// <summary>
    /// Urgent, unscheduled visit requiring immediate attention.
    /// </summary>
    Emergency = 3,

    /// <summary>
    /// Routine preventive health examination.
    /// </summary>
    Checkup = 4,

    /// <summary>
    /// Medical or surgical procedure.
    /// </summary>
    Procedure = 5,
}
