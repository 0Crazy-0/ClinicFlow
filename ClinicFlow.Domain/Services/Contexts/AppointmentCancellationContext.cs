using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the world state required to validate an appointment cancellation.
/// While the context itself is role-agnostic, it is primarily utilized to enforce patient-specific cancellation rules.
/// </summary>
public sealed record class AppointmentCancellationContext
{
    public required MedicalSpecialty Specialty { get; init; }
    public AppointmentPurpose Purpose { get; init; }
    public required FamilyMembership InitiatorMembership { get; init; }
    public AppointmentCategory RequestedCategory { get; init; }
}
