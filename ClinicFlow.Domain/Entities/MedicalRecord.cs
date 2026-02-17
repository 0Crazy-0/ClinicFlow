using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Events;
using ClinicFlow.Domain.Exceptions.MedicalRecords;

namespace ClinicFlow.Domain.Entities;

public class MedicalRecord : BaseEntity
{
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public Guid AppointmentId { get; init; }
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

    // Factory Method
    internal static MedicalRecord Create(Guid patientId, Guid doctorId, Guid appointmentId, string chiefComplaint)
    {
        if (patientId == Guid.Empty) throw new InvalidMedicalRecordException("Patient ID cannot be empty.");
        if (doctorId == Guid.Empty) throw new InvalidMedicalRecordException("Doctor ID cannot be empty.");
        if (appointmentId == Guid.Empty) throw new InvalidMedicalRecordException("Appointment ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(chiefComplaint)) throw new InvalidMedicalRecordException("Chief complaint cannot be empty.");

        var record = new MedicalRecord(patientId, doctorId, appointmentId, chiefComplaint);

        record.AddDomainEvent(new MedicalRecordCreatedEvent(record));

        return record;
    }

    // Domain Methods
    internal void AddDiagnosis(string diagnosis)
    {
        if (string.IsNullOrWhiteSpace(diagnosis)) throw new InvalidMedicalRecordException("Diagnosis cannot be empty.");

        Diagnosis = diagnosis;
    }

    internal void PrescribeTreatment(string treatment, string medications)
    {
        if (string.IsNullOrWhiteSpace(treatment)) throw new InvalidMedicalRecordException("Treatment cannot be empty.");
        if (string.IsNullOrWhiteSpace(medications)) throw new InvalidMedicalRecordException("Medications cannot be empty.");

        Treatment = treatment;
        Medications = medications;
    }

    internal void RecordLabResults(string labResults)
    {
        if (string.IsNullOrWhiteSpace(labResults)) throw new InvalidMedicalRecordException("Lab results cannot be empty.");

        LabResults = labResults;
    }

    internal void AddDoctorNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes)) throw new InvalidMedicalRecordException("Doctor notes cannot be empty.");

        DoctorNotes = notes;
    }

    internal void SetFollowUpInstructions(string instructions)
    {
        if (string.IsNullOrWhiteSpace(instructions)) throw new InvalidMedicalRecordException("Follow-up instructions cannot be empty.");

        FollowUpInstructions = instructions;
    }
}