namespace ClinicFlow.Domain.Services.Contexts.Cancellation;

public sealed record class StaffCancellationContext
{
    public Guid InitiatorUserId { get; init; }
    public DateTimeOffset CancelledAt { get; init; }
}
