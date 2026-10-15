using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Rescheduling;

public sealed record StaffReschedulingArgs
{
    public DateOnly NewDate { get; init; }
    public required TimeRange NewTimeSlot { get; init; }
}
