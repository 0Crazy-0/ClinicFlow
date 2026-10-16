using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.Scheduling;

/// <summary>
/// Encapsulates the context required exclusively for patient-initiated appointment scheduling.
/// </summary>
public sealed record class PatientSchedulingContext
{
    public required Patient TargetPatient { get; init; }
    public required AppointmentTypeDefinition AppointmentType { get; init; }
    public bool IsInitiatorPhoneVerified { get; init; }
    public IReadOnlyList<PatientPenalty> Penalties { get; init; } = [];
    public required Schedule DoctorSchedule { get; init; }
    public required FamilyMembership InitiatorMembership { get; init; }
    public required TimeZoneInfo ClinicTimeZone { get; init; }
}
