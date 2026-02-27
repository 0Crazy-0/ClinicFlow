using ClinicFlow.Domain.Common;
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

    public string Diagnosis { get; private set; } = string.Empty;

    public string Treatment { get; private set; } = string.Empty;

    public string Medications { get; private set; } = string.Empty;

    public string LabResults { get; private set; } = string.Empty;

    public string DoctorNotes { get; private set; } = string.Empty;

    public string FollowUpInstructions { get; private set; } = string.Empty;

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
    /// Records a clinical diagnosis for this encounter.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the diagnosis text is blank.</exception>
    internal void AddDiagnosis(string diagnosis)
    {
        if (string.IsNullOrWhiteSpace(diagnosis)) throw new DomainValidationException("Diagnosis cannot be empty.");

        Diagnosis = diagnosis;
    }

    /// <summary>
    /// Records the prescribed treatment and medications.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when either treatment or medications text is blank.</exception>
    internal void PrescribeTreatment(string treatment, string medications)
    {
        if (string.IsNullOrWhiteSpace(treatment)) throw new DomainValidationException("Treatment cannot be empty.");
        if (string.IsNullOrWhiteSpace(medications)) throw new DomainValidationException("Medications cannot be empty.");

        Treatment = treatment;
        Medications = medications;
    }

    /// <summary>
    /// Records laboratory results for this encounter.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the lab results text is blank.</exception>
    internal void RecordLabResults(string labResults)
    {
        if (string.IsNullOrWhiteSpace(labResults)) throw new DomainValidationException("Lab results cannot be empty.");

        LabResults = labResults;
    }

    /// <summary>
    /// Adds or replaces the doctor's clinical notes.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the notes text is blank.</exception>
    internal void AddDoctorNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) throw new DomainValidationException("Doctor notes cannot be empty.");

        DoctorNotes = notes;
    }

    /// <summary>
    /// Sets follow-up care instructions for the patient.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the instructions text is blank.</exception>
    internal void SetFollowUpInstructions(string instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions)) throw new DomainValidationException("Follow-up instructions cannot be empty.");

        FollowUpInstructions = instructions;
    }
}