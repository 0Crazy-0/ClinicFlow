using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Contexts.Rescheduling;

/// <summary>
/// Encapsulates the context required exclusively for patient-initiated appointment rescheduling.
/// </summary>
public sealed record class PatientReschedulingContext
{
    public required Patient TargetPatient { get; init; }
    public required AppointmentTypeDefinition AppointmentType { get; init; }
    public bool IsInitiatorPhoneVerified { get; init; }
    public IReadOnlyList<PatientPenalty> Penalties { get; init; } = [];
    public required Schedule DoctorSchedule { get; init; }
    public required FamilyMembership InitiatorMembership { get; init; }
    public LegalAuthorityType? CreatorLegalAuthority { get; init; }
    public required TimeZoneInfo ClinicTimeZone { get; init; }
}
