using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Entities
{
    public class Doctor : BaseEntity
    {
        public Guid UserId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public Guid MedicalSpecialtyId { get; set; }
        public string Biography { get; set; } = string.Empty;
        public int ConsultationRoomNumber { get; set; }
        
        public User User { get; set; }
        public MedicalSpecialty Specialty { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        
        public Doctor()
        {
            Schedules = [];
            Appointments = [];
        }
    }
}