using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required to validate and complete a medical encounter.
/// </summary>
public sealed class MedicalEncounterContext
{
    public Doctor ExpectedDoctor { get; init; } = null!;
    public Appointment Appointment { get; init; } = null!;
    public AppointmentTypeDefinition AppointmentTypeDefinition { get; init; } = null!;
    public IEnumerable<IClinicalDetailRecord> ProvidedDetails { get; init; } = [];
}
