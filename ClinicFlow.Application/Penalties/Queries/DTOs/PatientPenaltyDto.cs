namespace ClinicFlow.Application.Penalties.Queries.DTOs;

/// <param name="AppointmentId">The optional unique identifier of the appointment that triggered the penalty (e.g. no-show).</param>
/// <param name="Type">The category of the penalty (e.g. Warning, Suspension).</param>
/// <param name="BlockedUntil">The date indicating when a suspension block expires, null if no suspension is active.</param>
/// <param name="IsRemoved">A value indicating whether the penalty has been cleared or waived.</param>
public sealed record PatientPenaltyDto(
    Guid Id,
    Guid PatientId,
    Guid? AppointmentId,
    string Type,
    string Reason,
    DateOnly? BlockedUntil,
    bool IsRemoved
);
