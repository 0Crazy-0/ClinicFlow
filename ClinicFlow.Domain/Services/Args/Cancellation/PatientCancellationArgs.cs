using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record PatientCancellationArgs(
    Patient TargetPatient,
    Patient? InitiatorPatient,
    AppointmentCategory Category,
    MedicalSpecialty Specialty,
    string? Reason,
    DateTime CancelledAt
);
