using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Entities
{
    public class Patient : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string ChronicConditions { get; set; } = string.Empty;
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactPhone { get; set; } = string.Empty;

        public User User { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<MedicalRecord> MedicalRecords { get; set; }
        public ICollection<PatientPenalty> Penalties { get; set; }

        public Patient()
        {
            Appointments = [];
            MedicalRecords = [];
            Penalties = [];
        }
        public int GetAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;
            if (DateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }

        public bool IsBlockedFromBooking()
        {
            var activeBlock = Penalties.Where(p => p.PenaltyType == PenaltyType.TemporaryBlock)
            .Where(p => p.BlockedUntil.HasValue && p.BlockedUntil > DateTime.UtcNow).Any();

            return activeBlock;
        }
    }
}