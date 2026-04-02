using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record PatientCancellationArgs
{
    public required Patient TargetPatient { get; init; }
    public Patient? InitiatorPatient { get; init; }
    public AppointmentCategory Category { get; init; }
    public required MedicalSpecialty Specialty { get; init; }
    public string? Reason { get; init; }
    public DateTime CancelledAt { get; init; }
}
