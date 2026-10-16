namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record PatientCancellationArgs
{
    public string? Reason { get; init; }
}
