using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record StaffCancellationArgs(
    Guid InitiatorUserId,
    MedicalSpecialty Specialty,
    string Reason,
    DateTime CancelledAt
);
