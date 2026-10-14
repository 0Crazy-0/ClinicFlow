using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Rescheduling;

public sealed record DoctorReschedulingArgs
{
    public required Doctor InitiatorDoctor { get; init; }
    public DateOnly NewDate { get; init; }
    public required TimeRange NewTimeSlot { get; init; }
}
