using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Entities;

public class AppointmentType : BaseEntity
{
    public AppointmentTypeEnum Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TimeSpan DurationMinutes { get; private set; }
}
