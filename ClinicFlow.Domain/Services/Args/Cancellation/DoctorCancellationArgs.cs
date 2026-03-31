using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record DoctorCancellationArgs(
    Doctor? InitiatorDoctor,
    MedicalSpecialty Specialty,
    string? Reason,
    DateTime CancelledAt
);
