using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required exclusively for patient-initiated appointment scheduling.
/// </summary>
public sealed record class PatientSchedulingContext
{
    public IReadOnlyList<PatientPenalty> Penalties { get; init; } = [];
    public required Schedule DoctorSchedule { get; init; }
    public bool InitiatorHasAccessToTarget { get; init; }

    /// <summary>
    /// Holds the initiator's legal authority over the target patient.
    /// The guardian consent check compares this against None in the domain.
    /// </summary>
    public LegalAuthorityType InitiatorLegalAuthority { get; init; }
}
