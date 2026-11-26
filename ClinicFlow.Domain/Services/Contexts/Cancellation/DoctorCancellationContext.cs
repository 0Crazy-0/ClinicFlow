using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.Cancellation;

public sealed record class DoctorCancellationContext
{
    public required Doctor InitiatorDoctor { get; init; }
    public DateTimeOffset CancelledAt { get; init; }
}
