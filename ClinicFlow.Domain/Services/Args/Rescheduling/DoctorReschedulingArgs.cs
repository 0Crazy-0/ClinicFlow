using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Rescheduling;

public record DoctorReschedulingArgs
{
    public required Doctor InitiatorDoctor { get; init; }
    public DateTime NewDate { get; init; }
    public required TimeRange NewTimeRange { get; init; }
    public bool IsOverbook { get; init; }
}
