using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Scheduling;

public record DoctorSchedulingArgs
{
    public required Doctor InitiatorDoctor { get; init; }
    public required Patient TargetPatient { get; init; }
    public DateTime ScheduledDate { get; init; }
    public required TimeRange TimeRange { get; init; }
    public bool IsOverbook { get; init; }
}
