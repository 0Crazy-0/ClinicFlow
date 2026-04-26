namespace ClinicFlow.Domain.Enums;

/// <summary>
/// Classifies the severity of a penalty applied to a patient.
/// </summary>
public enum PenaltyType
{
    /// <summary>
    /// A non-blocking advisory notice issued to the patient.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// A time-limited block that prevents the patient from booking new appointments.
    /// </summary>
    TemporaryBlock = 2,
}
