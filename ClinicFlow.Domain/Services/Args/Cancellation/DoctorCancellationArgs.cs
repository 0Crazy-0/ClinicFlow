using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public record DoctorCancellationArgs(
    Doctor? InitiatorDoctor,
    MedicalSpecialty Specialty,
    string? Reason,
    DateTime CancelledAt
);
