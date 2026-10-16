using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.Cancellation;

/// <summary>
/// Encapsulates the world state required to validate an appointment cancellation.
/// While the context itself is role-agnostic, it is primarily utilized to enforce patient-specific cancellation rules.
/// </summary>
public sealed record class AppointmentCancellationContext
{
    public required Patient TargetPatient { get; init; }
    public required AppointmentTypeDefinition AppointmentType { get; init; }
    public required MedicalSpecialty Specialty { get; init; }
    public required FamilyMembership InitiatorMembership { get; init; }
    public DateTime CancelledAt { get; init; }
}
