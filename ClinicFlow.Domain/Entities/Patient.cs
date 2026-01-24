using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions;

namespace ClinicFlow.Domain.Entities
{
    public class Patient : BaseEntity
    {
        public Guid UserId { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public string BloodType { get; private set; } = string.Empty;
        public string Allergies { get; private set; } = string.Empty;
        public string ChronicConditions { get; private set; } = string.Empty;
        public string EmergencyContactName { get; private set; } = string.Empty;
        public string EmergencyContactPhone { get; private set; } = string.Empty;

        public User User { get; private set; }
        public ICollection<Appointment> Appointments { get; private set; }
        public ICollection<MedicalRecord> MedicalRecords { get; private set; }
        public ICollection<PatientPenalty> Penalties { get; private set; }

        private Patient() 
        {
            Appointments = [];
            MedicalRecords = [];
            Penalties = [];
        }

        public Patient(Guid userId, DateTime dateOfBirth, string bloodType, string allergies, string chronicConditions, string emergencyContactName, string emergencyContactPhone) : this()
        {
            UserId = userId;
            DateOfBirth = dateOfBirth;
            BloodType = bloodType;
            Allergies = allergies;
            ChronicConditions = chronicConditions;
            EmergencyContactName = emergencyContactName;
            EmergencyContactPhone = emergencyContactPhone;
        }

        public int GetAge()
        {
            var today = DateTime.Today;
            var age = today.Year - DateOfBirth.Year;

            if (DateOfBirth.AddYears(age) > today) age--;

            return age;
        }

        public bool IsBlockedFromBooking() => Penalties.Any(p => p.PenaltyType == PenaltyType.TemporaryBlock && p.BlockedUntil.HasValue && p.BlockedUntil > DateTime.UtcNow);

        public void EnsureNotBlocked()
        {
            if (IsBlockedFromBooking())
            {
                var blockedUntil = Penalties
                    .Where(p => p.PenaltyType == PenaltyType.TemporaryBlock && p.BlockedUntil > DateTime.UtcNow)
                    .Max(p => p.BlockedUntil) ?? DateTime.UtcNow;
                throw new PatientBlockedException(blockedUntil);
            }
        }
        
        public void AddPenalty(PatientPenalty penalty)
        {
            Penalties.Add(penalty);
        }
    }
}