using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities.ClinicalDetails;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a clinical encounter record linked to a specific appointment.
/// Captures diagnosis, treatment, lab results, and follow-up instructions.
/// </summary>
public class MedicalRecord : BaseEntity
{
    public Guid PatientId { get; init; }

    public Guid DoctorId { get; init; }

    public Guid AppointmentId { get; init; }

    /// <summary>
    /// Primary symptom or reason for the visit as reported by the patient.
    /// </summary>
    public string ChiefComplaint { get; private set; } = string.Empty;

    private readonly List<IClinicalDetailRecord> _clinicalDetails = [];

    /// <summary>
    /// A structured collection of clinical details (e.g., Cardiology flags, Dental odontograms, etc.) collected during the encounter.
    /// </summary>
    public IReadOnlyCollection<IClinicalDetailRecord> ClinicalDetails => _clinicalDetails.AsReadOnly();

    // EF Core constructor
    private MedicalRecord() { }

    private MedicalRecord(Guid patientId, Guid doctorId, Guid appointmentId, string chiefComplaint)
    {
        PatientId = patientId;
        DoctorId = doctorId;
        AppointmentId = appointmentId;
        ChiefComplaint = chiefComplaint;
    }

    /// <summary>
    /// Creates a new medical record and raises a <see cref="MedicalRecordCreatedEvent"/>.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when any required identifier is empty or the chief complaint is blank.</exception>
    internal static MedicalRecord Create(Guid patientId, Guid doctorId, Guid appointmentId, string chiefComplaint)
    {
        if (patientId == Guid.Empty) throw new DomainValidationException("Patient ID cannot be empty.");
        if (doctorId == Guid.Empty) throw new DomainValidationException("Doctor ID cannot be empty.");
        if (appointmentId == Guid.Empty) throw new DomainValidationException("Appointment ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(chiefComplaint)) throw new DomainValidationException("Chief complaint cannot be empty.");

        var record = new MedicalRecord(patientId, doctorId, appointmentId, chiefComplaint);

        record.AddDomainEvent(new MedicalRecordCreatedEvent(record));

        return record;
    }

    /// <summary>
    /// Adds a strongly-typed clinical detail to this medical record.
    /// Used primarily by the <see cref="Services.MedicalEncounterService"/> after enforcing domain policies.
    /// </summary>
    /// <param name="detail">The specific detail object containing medical data.</param>
    /// <exception cref="DomainValidationException">Thrown if the provided detail is null or a detail of that type was already added.</exception>
    internal void AddClinicalDetail(IClinicalDetailRecord detail)
    {
        if (detail is null) throw new DomainValidationException("Clinical detail cannot be null.");

        // It is a valid domain rule to prevent duplicate types of details per encounter (usually)
        if (_clinicalDetails.Any(d => d.TemplateCode == detail.TemplateCode))
            throw new DomainValidationException($"A clinical detail for template '{detail.TemplateCode}' already exists in this medical record.");

        _clinicalDetails.Add(detail);
    }

    /// <summary>
    /// Retrieves a specific clinical detail by its template code, or null if not present.
    /// Useful for queries or view models that need to extract specialized data (e.g., getting the 'VITALS' form).
    /// </summary>
    public IClinicalDetailRecord? GetClinicalDetail(string templateCode) => _clinicalDetails.FirstOrDefault(d => d.TemplateCode == templateCode);

}