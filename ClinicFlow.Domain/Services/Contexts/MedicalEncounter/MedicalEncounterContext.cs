using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts.MedicalEncounter;

/// <summary>
/// Encapsulates the context required to validate and complete a medical encounter.
/// </summary>
public sealed record class MedicalEncounterContext
{
    public required Doctor ExpectedDoctor { get; init; }
    public required Appointment TargetAppointment { get; init; }
    public required AppointmentTypeDefinition AppointmentType { get; init; }
    public DateTime CompletedAt { get; init; }
}
