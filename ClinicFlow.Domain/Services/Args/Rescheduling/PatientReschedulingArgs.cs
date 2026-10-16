using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Rescheduling;

public sealed record PatientReschedulingArgs
{
    public DateOnly NewDate { get; init; }
    public required TimeRange NewTimeSlot { get; init; }
    public string? NewPatientNotes { get; init; }
}
