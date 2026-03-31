using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Rescheduling;

public sealed record StaffReschedulingArgs
{
    public Guid InitiatorUserId { get; init; }
    public DateTime NewDate { get; init; }
    public required TimeRange NewTimeRange { get; init; }
    public bool IsOverbook { get; init; }
}
