using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Entities
{
    public class Schedule : BaseEntity
    {
        public Guid DoctorId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
        public Doctor Doctor { get; set; }
        
        public Schedule()
        {
            IsActive = true;
        }

        public bool IsAvailableAt(TimeSpan time) => IsActive && time >= StartTime && time < EndTime;

    }
}