using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Entities
{
    public class AppointmentType : BaseEntity
    {
        public AppointmentTypeEnum Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TimeSpan DurationMinutes { get; set; }

        public ICollection<Appointment> Appointments { get; set; }

        public AppointmentType()
        {
            Appointments = [];
        }
    }
}