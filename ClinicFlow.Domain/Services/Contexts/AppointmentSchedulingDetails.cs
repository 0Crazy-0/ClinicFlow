using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the core details requested for an appointment to be scheduled.
/// </summary>
public class AppointmentSchedulingDetails
{
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime ScheduledDate { get; init; }
    public TimeRange TimeRange { get; init; } = null!;
    public Guid AppointmentTypeId { get; init; }
}
