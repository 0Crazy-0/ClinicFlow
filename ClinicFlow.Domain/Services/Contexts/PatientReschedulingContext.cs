using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required exclusively for patient-initiated appointment rescheduling.
/// </summary>
public sealed record class PatientReschedulingContext
{
    public IReadOnlyList<PatientPenalty> Penalties { get; init; } = [];
    public required Schedule DoctorSchedule { get; init; }
    public required FamilyMembership InitiatorMembership { get; init; }
    public AppointmentCategory RequestedCategory { get; init; }
    public LegalAuthorityType? CreatorLegalAuthority { get; init; }
}
