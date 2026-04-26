namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record DoctorCancellationArgs
{
    public Guid InitiatorDoctorId { get; init; }
    public Guid InitiatorUserId { get; init; }
    public string? Reason { get; init; }
    public DateTime CancelledAt { get; init; }
}
