using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Scheduling;

public sealed record PatientSchedulingArgs
{
    public required Patient TargetPatient { get; init; }
    public required Patient InitiatorPatient { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime ScheduledDate { get; init; }
    public required TimeRange TimeRange { get; init; }
    public bool IsInitiatorPhoneVerified { get; init; }
}
