using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required to validate and cancel an appointment.
/// </summary>
public class AppointmentCancellationContext
{
    public User Initiator { get; init; } = null!;
    public AppointmentTypeDefinition AppointmentTypeDefinition { get; init; } = null!;
    public MedicalSpecialty Specialty { get; init; } = null!;
    public bool IsAuthorizedFamilyMember { get; init; }
    public string? Reason { get; init; }
}
