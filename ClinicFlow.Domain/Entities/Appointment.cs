using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Entities
{
    public class Appointment : BaseEntity
    {
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid AppointmentTypeId { get; set; }

        public DateTime ScheduledDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public AppointmentStatus Status { get; set; }
        public string PatientNotes { get; set; } = string.Empty;
        public string ReceptionistNotes { get; set; } = string.Empty; 

        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public Guid? CancelledByUserId { get; set; }

        public int RescheduleCount { get; set; }

        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public AppointmentType AppointmentType { get; set; }
        public MedicalRecord MedicalRecord { get; set; }

        public Appointment()
        {
            Status = AppointmentStatus.Scheduled;
            RescheduleCount = 0;
        }

        public bool CanBeCancelled(int minHoursBeforeAppointment)
        {
            var hoursUntilAppointment = (ScheduledDate - DateTime.UtcNow).TotalHours;
            return hoursUntilAppointment >= minHoursBeforeAppointment;
        }

        public bool CanBeRescheduled()
        {
            return RescheduleCount < 1 && Status is AppointmentStatus.Scheduled;
        }

        public void Cancel(Guid userId, string? reason, int minHours)
        {
            if (!CanBeCancelled(minHours))
            {
                Status = AppointmentStatus.LateCancellation;
            }
            else
            {
                Status = AppointmentStatus.Cancelled;
            }

            CancelledAt = DateTime.UtcNow;
            CancelledByUserId = userId;
            CancellationReason = reason;
        }

        public void Confirm()
        {
            Status = AppointmentStatus.Confirmed;
            ConfirmedAt = DateTime.UtcNow;
        }
    }
}