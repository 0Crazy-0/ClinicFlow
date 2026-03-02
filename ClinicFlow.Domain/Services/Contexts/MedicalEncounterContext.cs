using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Enums;
namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required to validate and complete a medical encounter.
/// </summary>
public class MedicalEncounterContext
{
    public Doctor ExpectedDoctor { get; init; } = null!;
    public Appointment Appointment { get; init; } = null!;
    public AppointmentType AppointmentCategory { get; init; }
    public MedicalSpecialty DoctorSpecialty { get; init; } = null!;
    public IEnumerable<IClinicalDetailRecord> ProvidedDetails { get; init; } = [];
}
