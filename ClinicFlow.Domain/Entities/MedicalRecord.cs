using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Entities
{
    public class MedicalRecord : BaseEntity
    {
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid AppointmentId { get; set; }

        public string ChiefComplaint { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Medications { get; set; } = string.Empty;
        public string LabResults { get; set; } = string.Empty;
        public string DoctorNotes { get; set; } = string.Empty;
        public string FollowUpInstructions { get; set; } = string.Empty;

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public Appointment Appointment { get; set; }
    }
}