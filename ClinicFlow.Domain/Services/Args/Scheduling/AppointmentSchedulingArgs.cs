using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Args.Scheduling;

/// <summary>
/// Encapsulates the core arguments requested for an appointment to be scheduled.
/// </summary>
public record AppointmentSchedulingArgs
{
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public DateTime ScheduledDate { get; init; }
    public TimeRange TimeRange { get; init; } = null!;
    public Guid AppointmentTypeId { get; init; }
}
