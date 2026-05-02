using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Rescheduling;

public sealed record PatientReschedulingArgs
{
    public required Patient TargetPatient { get; init; }
    public required Patient InitiatorPatient { get; init; }
    public DateTime NewDate { get; init; }
    public required TimeRange NewTimeRange { get; init; }
    public bool IsInitiatorPhoneVerified { get; init; }
}
