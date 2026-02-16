using ClinicFlow.Domain.Common;

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
}