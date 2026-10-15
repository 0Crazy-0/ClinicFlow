using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Scheduling;

public sealed record DoctorSchedulingArgs
{
    public DateOnly ScheduledDate { get; init; }
    public required TimeRange RequestedTimeSlot { get; init; }
    public bool HasGuardianConsentVerified { get; init; }
}
