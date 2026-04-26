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
    public IReadOnlyCollection<IClinicalDetailRecord> ClinicalDetails =>
        _clinicalDetails.AsReadOnly();

    // EF Core constructor
    private MedicalRecord() { }

    private MedicalRecord(Guid patientId, Guid doctorId, Guid appointmentId, string chiefComplaint)
    {
        PatientId = patientId;
        DoctorId = doctorId;
        AppointmentId = appointmentId;
        ChiefComplaint = chiefComplaint;
    }

    public static MedicalRecord Create(
        Guid patientId,
        Guid doctorId,
        Guid appointmentId,
        string chiefComplaint
    )
    {
        if (patientId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (doctorId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (appointmentId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (string.IsNullOrWhiteSpace(chiefComplaint))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        var record = new MedicalRecord(patientId, doctorId, appointmentId, chiefComplaint);

        record.AddDomainEvent(new MedicalRecordCreatedEvent(record));

        return record;
    }

    internal void AddClinicalDetail(IClinicalDetailRecord detail)
    {
        if (detail is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (_clinicalDetails.Any(d => d.TemplateCode == detail.TemplateCode))
            throw new DomainValidationException(DomainErrors.MedicalEncounter.DetailAlreadyExists);

        _clinicalDetails.Add(detail);
    }
}
