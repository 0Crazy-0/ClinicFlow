using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Rescheduling;

public record PatientReschedulingArgs
{
    public required Patient TargetPatient { get; init; }
    public required Patient InitiatorPatient { get; init; }
    public DateTime NewDate { get; init; }
    public required TimeRange NewTimeRange { get; init; }
}
