using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

public sealed record class DoctorCancellationContext
{
    public required Doctor InitiatorDoctor { get; init; }
    public DateTime CancelledAt { get; init; }
}
