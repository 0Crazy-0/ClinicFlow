using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required to validate and complete a medical encounter.
/// </summary>
public sealed record class MedicalEncounterContext
{
    public required Doctor ExpectedDoctor { get; init; }
    public required Appointment Appointment { get; init; }
    public required AppointmentTypeDefinition AppointmentTypeDefinition { get; init; }
    public IReadOnlyList<IClinicalDetailRecord> ProvidedDetails { get; init; } = [];
    public required DateTime CompletedAt { get; init; }
}
