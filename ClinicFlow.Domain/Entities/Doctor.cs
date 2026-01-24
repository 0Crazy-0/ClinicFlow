using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Entities
{
    public class Doctor : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string LicenseNumber { get; private set; } = string.Empty;
        public Guid MedicalSpecialtyId { get; private set; }
        public string Biography { get; private set; } = string.Empty;
        public int ConsultationRoomNumber { get; private set; }
        
        public User User { get; private set; }
        public MedicalSpecialty Specialty { get; private set; }
        public ICollection<Schedule> Schedules { get; private set; }
        public ICollection<Appointment> Appointments { get; private set; }
        
        private Doctor()
        {
            Schedules = [];
            Appointments = [];
        }

        public Doctor(Guid userId, string licenseNumber, Guid medicalSpecialtyId, string biography, int consultationRoomNumber) : this()
        {
            UserId = userId;
            LicenseNumber = licenseNumber;
            MedicalSpecialtyId = medicalSpecialtyId;
            Biography = biography;
            ConsultationRoomNumber = consultationRoomNumber;
        }
    }
}