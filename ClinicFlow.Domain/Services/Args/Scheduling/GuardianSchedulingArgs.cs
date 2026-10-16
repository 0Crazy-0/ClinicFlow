using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Scheduling;

public sealed record GuardianSchedulingArgs
{
    public Guid DoctorId { get; init; }
    public DateOnly ScheduledDate { get; init; }
    public required TimeRange RequestedTimeSlot { get; init; }
    public string? GuardianNotes { get; init; }
}
