using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required exclusively for patient-initiated appointment rescheduling.
/// </summary>
public sealed record class PatientReschedulingContext
{
    public IReadOnlyList<PatientPenalty> Penalties { get; init; } = [];
    public required Schedule DoctorSchedule { get; init; }
}
