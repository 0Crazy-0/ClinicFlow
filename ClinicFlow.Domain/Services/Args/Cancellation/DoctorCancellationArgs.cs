using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Args.Cancellation;

public sealed record DoctorCancellationArgs
{
    public Doctor? InitiatorDoctor { get; init; }
    public required MedicalSpecialty Specialty { get; init; }
    public string? Reason { get; init; }
    public DateTime CancelledAt { get; init; }
}
