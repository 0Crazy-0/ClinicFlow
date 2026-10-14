using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Scheduling;

public sealed record DoctorSchedulingArgs
{
    public required Doctor InitiatorDoctor { get; init; }
    public required Patient TargetPatient { get; init; }
    public DateOnly ScheduledDate { get; init; }
    public required TimeRange RequestedTimeSlot { get; init; }
    public bool HasGuardianConsentVerified { get; init; }
    public required TimeZoneInfo ClinicTimeZone { get; init; }
}
