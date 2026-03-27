using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Scheduling;

public record StaffSchedulingArgs
{
    public Guid InitiatorUserId { get; init; }
    public required Patient TargetPatient { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime ScheduledDate { get; init; }
    public required TimeRange TimeRange { get; init; }
    public bool HasGuardianConsentVerified { get; init; }
    public bool IsOverbook { get; init; }
}
