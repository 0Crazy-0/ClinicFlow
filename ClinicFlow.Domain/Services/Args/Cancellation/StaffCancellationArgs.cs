using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record StaffCancellationArgs
{
    public Guid InitiatorUserId { get; init; }
    public required MedicalSpecialty Specialty { get; init; }
    public required string Reason { get; init; }
    public DateTime CancelledAt { get; init; }
}
