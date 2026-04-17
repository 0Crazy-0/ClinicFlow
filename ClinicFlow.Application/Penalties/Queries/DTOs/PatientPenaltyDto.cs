namespace ClinicFlow.Application.Penalties.Queries.DTOs;

public sealed record PatientPenaltyDto(
    Guid Id,
    Guid PatientId,
    Guid? AppointmentId,
    string Type,
    string Reason,
    DateTime? BlockedUntil,
    bool IsRemoved
);
