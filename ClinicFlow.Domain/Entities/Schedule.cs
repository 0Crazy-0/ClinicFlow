using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

public class Schedule : BaseEntity
{
    public Guid DoctorId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeRange TimeRange { get; private set; }
    public bool IsActive { get; private set; }
    public Schedule()
    {
        IsActive = true;
    }
}
