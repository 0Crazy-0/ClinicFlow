using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Services.Contexts;

/// <summary>
/// Encapsulates the context required to schedule or reschedule an appointment.
/// </summary>
public class AppointmentSchedulingContext
{
    public IEnumerable<PatientPenalty> Penalties { get; init; } = [];
    public Schedule? DoctorSchedule { get; init; }
    public bool HasConflict { get; init; }
}
