using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Rescheduling;

public sealed record StaffReschedulingArgs
{
    public Guid InitiatorUserId { get; init; }
    public DateOnly NewDate { get; init; }
    public required TimeRange NewTimeRange { get; init; }
    public bool IsOverbook { get; init; }
}
