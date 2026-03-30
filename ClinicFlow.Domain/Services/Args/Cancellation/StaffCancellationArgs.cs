using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public record StaffCancellationArgs(
    Guid InitiatorUserId,
    MedicalSpecialty Specialty,
    string Reason,
    DateTime CancelledAt
);
