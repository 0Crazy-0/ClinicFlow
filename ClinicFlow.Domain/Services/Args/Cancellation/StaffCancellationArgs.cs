namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record StaffCancellationArgs
{
    public required string Reason { get; init; }
}
