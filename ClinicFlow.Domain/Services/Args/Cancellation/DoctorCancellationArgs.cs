namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record DoctorCancellationArgs
{
    public string? Reason { get; init; }
}
