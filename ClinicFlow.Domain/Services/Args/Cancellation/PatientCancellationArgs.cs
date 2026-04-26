using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record PatientCancellationArgs
{
    public required Patient TargetPatient { get; init; }
    public Guid InitiatorPatientId { get; init; }
    public string? Reason { get; init; }
    public DateTime CancelledAt { get; init; }
}
