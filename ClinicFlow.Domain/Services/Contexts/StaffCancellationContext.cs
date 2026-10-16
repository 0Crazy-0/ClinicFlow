namespace ClinicFlow.Domain.Services.Contexts;

public sealed record class StaffCancellationContext
{
    public Guid InitiatorUserId { get; init; }
    public DateTime CancelledAt { get; init; }
}
